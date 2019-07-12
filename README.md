# ctlg
![Build Status badge](https://ersh.visualstudio.com/_apis/public/build/definitions/c9754d86-e84f-486e-a3b3-f7f42d31c01d/1/badge)

A tool to catalog files and folders.

## Features
 - Calculates checksums. You can search files in catalog by checksum.
 - Supports hash functions: SHA-512, SHA-384, SHA-256, SHA-1, MD5, CRC32.
 - Can work with long paths.
 - Catalogs files inside archives. Supports common archive types: Zip, 7Zip, Rar.
 - Create file backups.

## Usage

    ctlg <command> [<args>]

    ctlg add -s <mask> -f <hash function> <directory>
    ctlg show <catalog entry IDs>
    ctlg find -f <hash function> -c <checksum> -s <size> -n <name pattern>
    ctlg list
    ctlg backup -f -n <backup name> -s <search pattern> <directory>
    ctlg restore -n <backup name> -d <date> <directory>

### Add

Scan directory and add files to the catalog.

Available options:

`-s <mask>`

`--search <mask>`

Include only files specified by `<mask>`. Can contain wildcards * and ?. Default is `*`.

`-f <hash function>`

`--function <hash function>`

Apply `<hash function>` to files when adding them to the catalog. Default is `SHA-1`.

### show

Show detailed information about catalog enty.

### find

Find file in the catalog. It is possible to search files by name, size, and checksum value.

### list

List all files in the catalog.

### backup

Backup files from specified directory.

### restore

Restore files from backup to specified directory.

## Implementation details

 - .NET console application.
 - Entity Framework with SQLite DB, database migrations in SQL.
 - Pri.LongPath is used to support long paths.
 - Autofac is used to inject dpendencies.
 - SharpCompress is used to read archive files.
 - Crc32.NET is used to calculate CRC32 checksums.
 - NUnit and Moq for testing.
 - Command and Event patterns.

## Runnig on Mono

 - Download System.Data.SQLite full source [`sqlite-netFx-source`](https://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki).
 - Run `Setup/compile-interop-assembly-release.sh`.
 - Copy `bin/2013/Release/bin/libSQLite.Interop.dylib` to Ctlg applicatio directory.

 [More info](https://stackoverflow.com/a/43173220/5642735).
