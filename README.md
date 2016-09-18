# ctlg
![Build Status badge](https://ersh.visualstudio.com/_apis/public/build/definitions/c9754d86-e84f-486e-a3b3-f7f42d31c01d/1/badge)

Catalog files and folders, search files by hash value.

## Usage

    ctlg <command> [<args>]

    ctlg add -s <mask> -f <hash function> <directory>
    ctlg find -h <hash>
    ctlg list

### Available commands:

 - add - Add directory to the catalog.
 - find - Find file in the catalog.
 - list - List all files in the catalog.

## Implementation details

 - .NET console application. 
 - Entity Framework with SQLite DB, database migrations in SQL.
 - Pri.LongPath is used to support long paths.
 - Autofac is used to inject dpendencies. 
 - NUnit and Moq for testing.
 - Command and Event patterns.
 
