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

    ctlg <command> [<options>]

### Add

    ctlg add -s <mask> -f <hash function> <directory>

Scan directory and add files to the catalog.

Available options:

    -s <mask>
    --search <mask>

Include only files specified by `<mask>`. Can contain wildcards * and ?. Default is `*`.

    -f <hash function>
    --function <hash function>

Apply `<hash function>` to files when adding them to the catalog. Default is `SHA-256`.

### show

    ctlg show <catalog entry IDs>

Show detailed information about catalog enty.

### find

    ctlg find -f <hash function> -c <checksum> -s <size> -n <mask>

Find file in the catalog. It is possible to search files by name, size, and checksum value.

Available options:

    -f <hash function>
    --function <hash function>

Specifies the `<hash function>`.

    -c <hash value>
    --checksum <hash value>

Specifies the `<hash value>`.

    -s <size>
    --size <size>

Size in bytes.

    -n <mask>
    --name <mask>

Include files by name matching `<mask>`.

### list

    ctlg list

List all files in the catalog.

### backup

    ctlg backup -f -n <backup name> -s <mask> <directory>

Backup files from specified directory.

Available options:

    -n <backup name>
    --name <backup name>

Backup name. Required.

    -s <mask>
    --search <mask>

Include files by name matching `<mask>`.

    -f
    --fast

Fast mode. Does not recalculate hashes if file was not modified since last backup.

### restore

    ctlg restore -n <backup name> -d <date> <directory>

Restore files from backup to specified directory.

Available options:

    -n <backup name>
    --name <backup name>

Backup name. Required.

    -d <date>
    --date <date>

Date when backup was taken in `yyyy-MM-dd_HH-mm-ss` format. It is possible to skip date parts from the right e.g.
`yyyy-MM-dd_HH-mm`, `yyyy-MM-dd` are laso valid.


## Implementation details

 - .NET console application (.NET Framework 4.6.2).
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
