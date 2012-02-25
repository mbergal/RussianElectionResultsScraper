Set oShell = CreateObject("WScript.Shell")
Set ofso = CreateObject("Scripting.FileSystemObject")
oShell.CurrentDirectory = oFSO.GetParentFolderName(Wscript.ScriptFullName)

args = ""
Set shell = CreateObject("Shell.Application")
shell.ShellExecute "uninstallPSCmdlets.cmd", Wscript.Arguments(0), "", "runas"
