// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

/* ---------------------------
 * NO
 * ------------------------ */

// We prefer to list all parameters
// Example: private void lnkManual_LinkClicked(object sender, EventArgs e) - Parameter 'sender' is never used
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("", "RECS0154")]

// Raised on Visual Studio generated code - Warns about calls to virtual member functions occuring in the constructor
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("", "RECS0021")]

// 'else redundant', best readable with.
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("", "RECS0147")]

// Convert nested if else to 'switch' statement. We prefer the nested if
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("", "RECS0012")]

// Redundant condificon check before assignment - Many of them are WinForms<>Mono optimization
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("", "RECS0134")]

// Populate switch - Why i need to add code to default or missing, if never happen?
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("", "IDE0010")]

// Doesn't matter, better clean code.
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("", "RECS0122")]

/* ---------------------------
 * TOFIX
 * ------------------------ */

// Culture-aware issues
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("", "RECS0060")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("", "RECS0062")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("", "RECS0063")]


