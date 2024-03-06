// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
//
// Eddie is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Eddie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Eddie. If not, see <http://www.gnu.org/licenses/>.
// </eddie_source_header>

#include <fcntl.h>
#include <fstream>
#include <iostream>
#include <map>
#include <memory>
#include <mutex>
#include <sstream>
#include <string.h>
#include <sys/select.h>
#include <sys/stat.h>
#include <thread>
#include <vector>

#include <libayatana-appindicator/app-indicator.h>

#pragma GCC diagnostic push
// GTK just doesn't intend to support image menu items going forward. They're considered bad practice in modern UI.
// All deprecation warning are related to menu icons.
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/*
Icon embedding:
    1) Convert the image:
        gdk-pixbuf-csource --raw --name=myimage_inline myimage.png > myimage.h

    2) Next, include your newly created header into your app, and build your GtkImage using

        #include "myimage.h"
        ...
        GdkPixbuf* pixbuf = gdk_pixbuf_new_from_inline (-1, myimage_inline, FALSE, nullptr);
        GtkWidget* logo = gtk_image_new_from_pixbuf(pixbuf);
*/

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/*
// Change the status of the tray icon:
    echo "tray.active:true" > /tmp/eddie_tray.in
    echo "tray.active:false" > /tmp/eddie_tray.in

// Change the label of a menu item:
    echo "menu.status.text:foo" > /tmp/eddie_tray.in

// Change the icon of a menu item with a system stock
    echo "menu.status.icon:stock:gtk-quit" > /tmp/eddie_tray.in

// Change the icon of a menu item with a custom resource
    echo "menu.status.icon:res/info.png" > /tmp/eddie_tray.in

// Disable a menu item
    echo "menu.status.enable:false" > /tmp/eddie_tray.in

// Hide a menu item
    echo "menu.status.visible:false" > /tmp/eddie_tray.in

// Notify the application to exit
    echo "action.exit" > /tmp/eddie_tray.in
*/

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#define TRAY_ICON_FILE_NORMAL       "/icons/appindicator.png"
#define TRAY_ICON_FILE_GRAY         "/icons/appindicator-gray.png"

#define MENU_ITEM_DATA_ID           "id"
#define MENU_ITEM_TIMEOUT_SEC       2                      // If a command is not processed within this value, the app will exit

#define FIFO_MENU_STATUS            "menu.status"
#define FIFO_MENU_CONNECT           "menu.connect"
#define FIFO_MENU_PREFERENCES       "menu.preferences"
#define FIFO_MENU_ABOUT             "menu.about"
#define FIFO_MENU_RESTORE           "menu.restore"
#define FIFO_MENU_EXIT              "menu.exit"

#define FIFO_TRAY_ACTIVE            "tray.active"
#define FIFO_ACTION_PING            "action.ping"
#define FIFO_ACTION_EXIT            "action.exit"

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

static AppIndicator *g_indicator = nullptr;

static std::string empty_string;

static std::string g_rootPath;

typedef std::map<std::string, GtkWidget *> ImagesMap;
typedef std::map<std::string, GtkWidget *> MenusMap;

static ImagesMap g_imagesMap;
static MenusMap g_menusMap;

static bool g_quitCalled = false;
static std::mutex g_quitMutex;

static time_t g_watchTimer = 0;
static std::mutex g_timerMutex;

static std::unique_ptr<std::fstream> g_fifoRead;
static std::unique_ptr<std::fstream> g_fifoWrite;
static std::mutex g_fifoWriteMutex;
static int g_fifoWatch = -1;

static std::unique_ptr<std::thread> g_fifoThread;
static std::unique_ptr<std::thread> g_watchThread;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

template <typename T>
inline bool starts_with(const std::basic_string<T> &str, const std::basic_string<T> &pattern)
{
    const typename std::basic_string<T>::size_type str_size = str.size();
    const typename std::basic_string<T>::size_type pattern_size = pattern.size();
    if(str_size < pattern_size)
        return false;

    return (str.compare(0, pattern_size, pattern) == 0);
}

static std::string getFullPath(const std::string &relativePath)
{
    if(relativePath.empty())
        return empty_string;

    if(relativePath[0] == '/')
        return g_rootPath + relativePath;

    return g_rootPath + "/" + relativePath;
}

static GtkWidget * loadImage(const std::string &filename)
{
    std::string fullPath = getFullPath(filename);

    ImagesMap::const_iterator i = g_imagesMap.find(fullPath);
    if(i != g_imagesMap.end())
        return i->second;

    GtkWidget *image = gtk_image_new_from_file(fullPath.c_str());
    if(image == nullptr)
        return nullptr;

    g_imagesMap[fullPath] = image;
    return image;
}

static GtkWidget * getStockImage(const std::string &imageName)
{
    return gtk_image_new_from_icon_name(imageName.c_str(), GTK_ICON_SIZE_MENU);
}

static GtkWidget * parseImageCommand(const std::string &args)
{
    static std::string stock_pattern = "stock:";
    if(starts_with(args, stock_pattern))
    {
        std::string stockImageName = args.substr(stock_pattern.size());
        return getStockImage(stockImageName);
    }

    return loadImage(args);
}

static void updateMenuItemLabel(GtkWidget *menuItem, const std::string &label)
{
    if(menuItem == nullptr || label.empty())
        return;

    gtk_menu_item_set_label(&GTK_IMAGE_MENU_ITEM(menuItem)->menu_item, label.c_str());
}

static void updateMenuItemImage(GtkWidget *menuItem, GtkWidget *icon)
{
    if(menuItem == nullptr || icon == nullptr)
        return;

    gtk_image_menu_item_set_image(GTK_IMAGE_MENU_ITEM(menuItem), icon);    
}

static void updateMenuItemEnable(GtkWidget *menuItem, bool enable)
{
    if(menuItem != nullptr)
        gtk_widget_set_sensitive(menuItem, enable ? TRUE : FALSE);
}

static void updateMenuItemVisible(GtkWidget *menuItem, bool visible)
{
    if(menuItem != nullptr)
        gtk_widget_set_visible(menuItem, visible ? TRUE : FALSE);
}

static void clearImages()
{
    for(ImagesMap::iterator i = g_imagesMap.begin(); i != g_imagesMap.end(); ++i)
    {
        gtk_image_clear(GTK_IMAGE(i->second));
    }

    g_imagesMap.clear();
}

static void writeOutput(const std::string &cmd)
{
    std::unique_lock<std::mutex> lock(g_fifoWriteMutex);
    *g_fifoWrite << cmd << std::endl;
}

static bool getQuit()
{
    std::unique_lock<std::mutex> lock(g_quitMutex);
    return g_quitCalled;
}

static void doQuit()
{
    {
        std::unique_lock<std::mutex> lock(g_quitMutex);
        if(g_quitCalled)
            return;

        g_quitCalled = true;
    }

    gtk_main_quit();
}

static void onMenuItemClick(GtkWidget *widget, gpointer data)
{
    const char *menuItemID = static_cast<const char *>(data);
    if(menuItemID != nullptr)
        writeOutput(menuItemID);
}

static void quitHandler(int e)
{
    doQuit();
}

static GtkWidget * createMenuItem(const std::string &id, const std::string &label, GtkWidget *parent, GtkWidget *icon = nullptr, void (* callback)(GtkWidget *widget, gpointer data) = nullptr)
{
    MenusMap::const_iterator m = g_menusMap.find(id);
    if(m != g_menusMap.end())
        return m->second;

    GtkWidget *menuItem = gtk_image_menu_item_new_with_label(label.c_str());
    if(menuItem == nullptr)
        return nullptr;

    std::pair<MenusMap::iterator, bool> ret = g_menusMap.insert(std::make_pair(id, menuItem));
    if(!ret.second)
        return nullptr;


    gtk_image_menu_item_set_always_show_image(GTK_IMAGE_MENU_ITEM(menuItem), TRUE);
    
    if(icon != nullptr) gtk_image_menu_item_set_image(GTK_IMAGE_MENU_ITEM(menuItem), icon);

    if(parent != nullptr)
        gtk_menu_shell_append(GTK_MENU_SHELL(parent), menuItem);

    if(callback == nullptr)
        callback = &onMenuItemClick;

    g_signal_connect(menuItem, "activate", G_CALLBACK(callback), const_cast<char *>(ret.first->first.c_str()));

    gtk_widget_show(menuItem);

    return menuItem;
}

static GtkWidget * addMenuSeparator(GtkWidget *parent)
{
    GtkWidget *separator = gtk_separator_menu_item_new();
    if(separator == nullptr)
        return nullptr;

    gtk_menu_shell_append(GTK_MENU_SHELL(parent), separator);

    return separator;
}

static void printUsage(char *argv[])
{
    std::cerr << "Don't execute this app directly, it's used by Eddie" << std::endl;
}

static bool parseCommand(const std::string &str, std::string &cmd, std::string &args)
{
    size_t pos = str.find(":");
    if(pos == std::string::npos)
    {
        cmd = str;
    }
    else
    {
        cmd = str.substr(0, pos);
        args = str.substr(pos + 1, std::string::npos);
    }

    return true;
}

static void updateTrayStatus(bool active)
{
    if(g_indicator == nullptr)
        return;

    app_indicator_set_icon(g_indicator, getFullPath(active ? TRAY_ICON_FILE_NORMAL : TRAY_ICON_FILE_GRAY).c_str());
}

static void handleFifoCommandUI(const std::string &str)
{
    std::string cmd;
    std::string args;
    parseCommand(str, cmd, args);

    if(cmd == FIFO_TRAY_ACTIVE)
    {
        updateTrayStatus(args == "true");
    }
    else
    {
        for(MenusMap::const_iterator i = g_menusMap.begin(); i != g_menusMap.end(); ++i)
        {
            // Check if the command is relative to the current menu item
            if(starts_with(cmd, i->first))
            {
                if(cmd == i->first + ".text")
                    updateMenuItemLabel(i->second, args);
                else if(cmd == i->first + ".icon")
                    updateMenuItemImage(i->second, parseImageCommand(args));
                else if(cmd == i->first + ".enable")
                    updateMenuItemEnable(i->second, args == "true");
                else if(cmd == i->first + ".visible")
                    updateMenuItemVisible(i->second, args == "true");

                break;
            }
        }
    }
}

static gboolean handleFifoCommandImpl(gpointer data)
{
    if(data != nullptr)
    {
        handleFifoCommandUI(static_cast<const char *>(data));
        free(data);
    }

    return FALSE; // Remove the function from the events loop
}

static bool handleFifoCommand(const std::string &cmd)
{
    if(cmd == FIFO_ACTION_EXIT)
    {
        doQuit();
        return false;
    }

    // Let the command to be processed in the GTK thread
    g_idle_add(handleFifoCommandImpl, strdup(cmd.c_str()));
    return true;
}

static void parseFifoCommands()
{
    for(;;)
    {
        std::string cmd;
        //*g_fifoRead >> cmd;
        std::getline(*g_fifoRead, cmd);

        if(cmd.empty())
            break;  // Could happen when the Fifo has been closed

        if(!handleFifoCommand(cmd))
            break;
    }
}

static bool isFifoReadable()
{
    struct timeval timeout;
    timeout.tv_sec = 0;
    timeout.tv_usec = 100;

    fd_set fread;
    FD_ZERO(&fread);
    FD_SET(g_fifoWatch, &fread);

    if(select(g_fifoWatch + 1, &fread, nullptr, nullptr, &timeout) < 0)
        return false;

    return FD_ISSET(g_fifoWatch, &fread);
}

static void watchCommands()
{
    for(;;)
    {
        sleep(1);

        if(getQuit())
            break;

        {
            std::unique_lock<std::mutex> lock(g_timerMutex);

            // Check if there could be a pending read
            if(g_watchTimer != 0)
            {
                if(isFifoReadable())
                {
                    double elapsed = difftime(time(nullptr), g_watchTimer);
                    if(elapsed >= MENU_ITEM_TIMEOUT_SEC)
                    {
                        doQuit();
                        break;
                    }
                }
                else
                {
                    g_watchTimer = 0;
                }
            }
            else
            {
                writeOutput(FIFO_ACTION_PING);
                g_watchTimer = time(nullptr);
            }
        }
    }
}

static void runUI(int argc, char *argv[])
{
    gtk_init(&argc, &argv);

    std::string icon = getFullPath(TRAY_ICON_FILE_GRAY);

    g_indicator = app_indicator_new("eddie-tray", icon.c_str(), APP_INDICATOR_CATEGORY_APPLICATION_STATUS);
    app_indicator_set_status(g_indicator, APP_INDICATOR_STATUS_ACTIVE);
    app_indicator_set_icon(g_indicator, icon.c_str());

    GtkWidget *menu = gtk_menu_new();
    
    createMenuItem(FIFO_MENU_STATUS, "", menu, getStockImage("gtk-no"));
    createMenuItem(FIFO_MENU_CONNECT, "", menu, getStockImage("gtk-go-forward"));
    addMenuSeparator(menu);
    createMenuItem(FIFO_MENU_PREFERENCES, "Preferences", menu, getStockImage("gtk-preferences"));
    createMenuItem(FIFO_MENU_ABOUT, "About", menu, getStockImage("gtk-info"));
    addMenuSeparator(menu);
    createMenuItem(FIFO_MENU_RESTORE, "Restore", menu, getStockImage("gtk-redo"));
    //addMenuSeparator(menu);
    createMenuItem(FIFO_MENU_EXIT, "Exit", menu, getStockImage("gtk-quit"));

    app_indicator_set_menu(g_indicator, GTK_MENU(menu));

    gtk_widget_show_all(menu);

	signal(SIGINT, quitHandler);
	signal(SIGTERM, quitHandler);
	signal(SIGABRT, quitHandler);

    gtk_main();

    g_menusMap.clear();
    clearImages();

    g_indicator = nullptr;
}

static std::string getCurrentPath()
{
    char buff[PATH_MAX];
    memset(buff, 0, PATH_MAX);
    if(!getcwd(buff, PATH_MAX))
        return "";

    return buff;
}

static bool sendExitMessage(const std::string &filename)
{
    // Do NOT reuse g_fifoRead here while a possibile pending read is done in another thread

    std::fstream tmp(filename.c_str());
    if(!tmp.is_open())
    {
        // std::cerr << "Failed to open '" << filename << "'";
        return false;
    }

    tmp << FIFO_ACTION_EXIT << std::endl;
    return true;
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

int main(int argc, char *argv[])
{
    g_rootPath = getCurrentPath();
    if(g_rootPath.empty())
    {
        // std::cerr << "Failed to get the current path" << std::endl;
        return -1;
    }

    if(argc < 2)
    {
        printUsage(argv);
        return -1;
    }
    
    std::string inputFile;
    std::string outputFile;

    int opt;
    while((opt = getopt(argc, argv, "r:w:p:")) != -1)
    {
        switch(opt)
        {
        case 'r':   inputFile = optarg;
                    break;

        case 'w':   outputFile = optarg;
                    break;

        case 'p':   g_rootPath = optarg;
                    break;

        default:    printUsage(argv);
                    return 1;
        }
    }

    unlink(inputFile.c_str());
    unlink(outputFile.c_str());

    if(mkfifo(inputFile.c_str(), 0666) != 0)
    {
        // std::cerr << "Failed to create '" << inputFile << "' input fifo";
        return -1;
    }

    if(mkfifo(outputFile.c_str(), 0666) != 0)
    {
        // std::cerr << "Failed to create '" << outputFile << "' output fifo";
        return -1;
    }

    g_fifoRead.reset(new std::fstream(inputFile.c_str()));
    if(!g_fifoRead->is_open())
    {
        // std::cerr << "Failed to open '" << inputFile << "'";
        return -1;
    }
    g_fifoWrite.reset(new std::fstream(outputFile.c_str()));
    if(!g_fifoWrite->is_open())
    {
        // std::cerr << "Failed to open '" << outputFile << "'";
        return -1;
    }
    g_fifoWatch = open(outputFile.c_str(), O_RDONLY);
    if(g_fifoWatch == -1)
    {
        // std::cerr << "Failed to create watcher of '" << outputFile << "'";
        return -1;
    }

    g_fifoThread.reset(new std::thread(parseFifoCommands));
    g_fifoThread->detach();
    g_watchThread.reset(new std::thread(watchCommands));
    g_watchThread->detach();

    runUI(argc, argv);

    if(!sendExitMessage(inputFile))
    {
        unlink(inputFile.c_str());
        unlink(outputFile.c_str());

        exit(-1);
    }

    //g_watchThread->join(); // 2.24.2, use detach() above, otherwise dump in cout a 'terminate called without an active exception'
    //g_fifoThread->join(); // 2.24.2, use detach() above

    close(g_fifoWatch);
    g_fifoWatch = -1;
    g_fifoRead->close();
    g_fifoWrite->close();

    unlink(inputFile.c_str());
    unlink(outputFile.c_str());

    return 0;
}

#pragma GCC diagnostic pop