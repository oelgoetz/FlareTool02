Description

The program merges multiple Flare projects that are linked via TOC (Table of Content) Entries into a single one.
There are several reasons why this is necessary:
1. Having all the current documentation in one big Flare project significantly slows down the Flare desktop application when you have to work with it.
2. The older Flare output formats allowed to merge projects at build time - it was possible to link from a Flare TOC file (.fltoc) to a target file (.fltar) of another Flare project.
When this feature was used, the Flare compiler built both projects and linked the subproject output into the main project output.
However the newer Flare output formats do not support this feature anymore.

Please note: The program is a command-line tool and you have to pass parameters as arguments.

Params 1 to 4 are obligatory:
 1. Name of the Build project (Currently there are only two options: 'GLHelp' or 'V4Help')
 2. File path where all the Flare project merging will take place. I.e.: the local path where the source projects are stored.
 3. Language ('00', '01', '02', '03' or '17') - the language versions must be a child folder of the previous path.
    '17' is currently only available for GLHelp)
 4. The name of the "main" project
    (i.e. the project that delivers all common resources for the build project - currently it is always 'Main')

All other parameters are optional:
Optional parameters must always start with a '-' 
With no further arguments set, the program will only analize the current state of the source projects that contribute to the desired Build project.
It will print the results on the screen, closing the console after finishing.

 '-copy' copies all required files into the build project that is specified in argument (1).
   If a folder with the build project name already exists in the path specified by (2) and (3), it will be deleted completely before the copy process starts
 
 '-build' includes functionality of -copy but in addition it causes the Flare compiler to build the Online Help target of the build project 
   after the copy process is finished.
   Remark: If you want to use the -build option, make sure that MadCap Flare is installed on the machine and include 'madbuild.exe' in your PATH variable.    
 
 '-wait' leaves the console open after the program is finished
	
Example: 
FlareTool01 V4Help C:\docuR2022\docusrc 00 Main -build -wait
	
will create the V4Help Build target in the Flder C:\docuR2022\docusrc\00
it will start the search in the project C:\docuR2022\docusrc\00\Main,
and it will copy all required resources from Main and all the files in other projects that are referenced from there into the project V4Help. 
The copied files will be modified so that output can be built from the V4Help project.
If C:\docuR2022\docusrc\00\V4Help is already existing, it will delete this folder and all its content before copying.
When everything is copied, the console window will be left open and the WebHelp Target in the Build project V4Help will be built.