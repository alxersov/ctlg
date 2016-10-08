# ctlg
![Build Status badge](https://ersh.visualstudio.com/_apis/public/build/definitions/c9754d86-e84f-486e-a3b3-f7f42d31c01d/1/badge)

A tool to catalog files and folders.

## Features
 - Calculates checksums. You can search files in catalog by checksum.
 - Supports hash functions: SHA-512, SHA-384, SHA-256, SHA-1, MD5, CRC32. 
 - Can work with long paths. 
 - Catalogs files inside archives. Supports common archive types: Zip, 7Zip, Rar.

## Usage

    ctlg <command> [<args>]

    ctlg add -s <mask> -f <hash function> <directory>
    ctlg show <catalog entry IDs>
    ctlg find -f <hash function> -c <checksum> -s <size> -n <name pattern>
    ctlg list

### Available commands

 - add - Scan directory and add files to the catalog.
 - show - Show detailed information about catalog enty.
 - find - Find file in the catalog. It is possible to search files by name, size, and checksum value.
 - list - List all files in the catalog.

## Implementation details

 - .NET console application. 
 - Entity Framework with SQLite DB, database migrations in SQL.
 - Pri.LongPath is used to support long paths.
 - Autofac is used to inject dpendencies.
 - SharpCompress is used to read archive files.
 - Crc32.NET is used to calculate CRC32 checksums.
 - NUnit and Moq for testing.
 - Command and Event patterns.
 
