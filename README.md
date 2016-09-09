# ctlg
![Build Status badge](https://ersh.visualstudio.com/_apis/public/build/definitions/c9754d86-e84f-486e-a3b3-f7f42d31c01d/1/badge)

Catalog files and folders, search files by hash value.

## Usage

    ctlg.exe add <path to directory>
    ctlg.exe find <hash>
    ctlg.exe list

## Implementation details

 - .NET console application. 
 - Entity Framework with SQLite DB, database migrations in SQL.
 - Autofac is used to inject dpendencies. 
 - NUnit and Moq for testing.
 - Command and Event patterns.
 
