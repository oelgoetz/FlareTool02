# Introduction 
This application creates a Flare Build project from a number of Flare projets by copying and fetching all necessary files.

# Getting Started
1.	Installation process: Not necessary.
2.	Software dependencies: If you want to create output you need a flare compiler istalled.
	madbuild.exe must be defined in your PATH variable
3.	Latest releases: Not defined.
4.	API references: Not defined.

# Build and Test
Command Line arguments:
1: Build type. Possible values:
-V4Help
-GLHelp
2: Repository. Possible values:
(This may depend on the path where your repositories are stored locally. It is assumed that they all are direct children of C:\)
-docu: git master
-docuR2022, docuR2020, docuR2019 and so on ...
3: Language. Possible values: 00 01 02 03 ... 17 is only available for GLHelp
4: The Flare project where the master target and toc file are located
5: Shut the Console window when done. Possible values:
-quit
-wait

Other parameters are defined in the source code.

# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)
