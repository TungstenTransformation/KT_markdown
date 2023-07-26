# Kofax Transformation Markdown
 
This converts a Kofax Transformation fpr file into a human readable and machine-diffable markdown file.  
Each class is written to its own markdown file in the subfolder *md* of the project folder.  
Useful for storing in github and seeing script and config changes.
Supports
-class structure, fields, tables, locator names
-formatters, validation rules, dictionary and database names
not included
-internal config settings of locators, formatters, validation rules.

## installation
* [Download](https://github.com/KofaxTransformation/KT_markdown/releases/download/1.0.2/KT_markdown.exe) Kofax Transformation Markdown
* At commandline type **KT_markdown.exe [fpr_filename] **
```batch
KT_markdown.exe c:\temp\ktproject\ktproject.fpr
```
Your markdown files will be created in the subfolder **md** in the project directory. They can be viewed and diffed in Visual Studio Code or in GitHub.

## 1.0.2
* write scripts to separate .vb files.
* delete old files if class name changes.
* only update files if file changed. This preserves useful "Date Modified" on files in File System.
* highlight default OCR profiles with *
* Show field info in a table including thresholds
## 1.0.1 (25 July 2023) 
* writes a .md for each class.
* internal hyperlinks.
* shows mappings from locators to fields.
* shows subfields of locators


