# Kofax Transformation Automatic Documentation
 
This converts a Kofax Transformation Project file into a human readable and machine-diffable markdown file.  
Each class is written to its own markdown file in the subfolder *md* of the project folder.  
Useful for storing in GitHub and seeing script and config changes.  
Supports
* class structure, fields, tables, locator names
* formatters, validation rules, dictionary and database names  

not included yet 
* properties of locators, formatters, validation rules, databases and dictionaries.  

Compatible with KTM, KTA and RPA.  

## Installation
* [Download](https://github.com/KofaxTransformation/KT_markdown/releases/download/1.0.2/KT_markdown.exe) Kofax Transformation Markdown.
* Copy **KT_Markdown.exe** into your Project Folder.
* Double-click **KT_Markdown.exe**.
* Your markdown files will be created in the subfolder **md** in the project directory. 
* They can be viewed and diffed in Visual Studio Code or in GitHub.

## Advanced Features
* At commandline type **KT_markdown.exe [fpr_filename] [outputfoldername]**
```batch
KT_markdown.exe c:\temp\ktproject\
```

## Compilation
To compile into a standalone executable without any dlls do the following.
* Meun **Build/Publish Section**.
* Click **Publish**.
* The version number increments itself.

Both parameters are optional. It works out what you mean.  
## 1.0.4 (11 Sep 2023)
* ScriptVariables.xml file is optional
## 1.0.3 (1 Aug 2023)
* Fixed project scripts.
* improved console logging.
## 1.0.2 (26 July 2023)
* write scripts to separate .vb files.
* delete old files if class name changes.
* only update files if file changed. This preserves useful "Date Modified" on files in File System.
* highlight default OCR profiles and Formatters with *.
* Show field info in a table including thresholds.
## 1.0.1 (25 July 2023) 
* writes a .md for each class.
* internal hyperlinks.
* shows mappings from locators to fields.
* shows subfields of locators


