# CilDllEditor
Simple tool to edit managed (C#, etc.) DLLs by editing their IL code

## Usage:
This tool currently support 2 modes of operation.
### 1. Manual mode
Usage: `CilDllEditor myDllName.dll [myKeyFileName.snk]`
Both arguments can be absolute or relative paths too. Both files have to have the given extensions: currently only dll and snk files are supported. The key argument is optional, if signing is not necessary it can be omitted.
 The tool executes these steps:
 1. Disassembles the given dll to `disassembledDll.il` with ildasm.
 2. Opens `disassembledDll.il` with Notepad. Waits until Notepad is closed.
 3. The user is expected to edit the IL code, save and close Notepad at which point the tool will continue. It's also possible to close Notepad without changes, in this case the DLL will be equivalent (but not identical) to the original.
 4. A backup will be created of the DLL before overwriting.
 5. The IL will be reassambled into the DLL using ilasm, overwriting the original.
 ### 2. Auto path mode
 Usage: `CilDllEditor`
 In this mode the tool will patch the DLL in a predifined manner without manual interactions. The two cmd arguments of the Manual mode cannot be used here.
 Someone is expected to define the patch and then distribute it, let's call him 'admin'. The goal of this mode is to be able to distribute a patch, that can automatically patch a DLL regardless of the DLL version. As long as the search term is found in the DLL, it will always be replaced, regardless of the other changes in the DLL.
 The admin has to open the released EXE file with a text editor, navigate to the bottom and add the predefined behavior there. This is called the (auto patch) data section.
 The data section has the following parts:
 1. CilDllEditorAutoPatchData_Begin: a fixed header to be able to find the data section.
 2. dll: a fixed label that indicates, the next line will contain the name/path of the dll that will be patched.
 3. the name of the dll to be patched
 4. key: a fixed label that indicates, the next line will contain the name/path of the snk file that will be used for signing. This is optional, if omitted, the next line must also be omitted.
 5. searchTerm: a fixed label that indicates, whatever follows this in the next lines will be used as search term. The search term will be replaced in the IL code with the replacement term.
 6. here goes the search term that can spread multiple lines
 7. searchTermEnd_replaceWith: a fixed label that indicates, the search term ended and whatever follows this in the next lines will be used as replacement term. The search term will be replaced with this.
 Example:
 ```
 CilDllEditor.exe
 
 ...............................
 .........binary....data........
 ....................... 
 CilDllEditorAutoPatchData_Begin
dll
myDllName.dll
key
..\..\..\signatures\myKeyFileName.snk
searchTerm
  .method public hidebysig newslot specialname virtual final 
          instance bool get_myProperty() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      bool myNameSpace.MyClass::myField
    IL_0006:  ret
  } // end of method MyClass::get_myProperty
searchTermEnd_replaceWith
  .method public hidebysig newslot specialname virtual final 
          instance bool  get_myProperty() cil managed
  {
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldc.i4.0
    IL_0001:  ret
  } // end of method

 ```
