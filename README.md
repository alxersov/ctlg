# ctlg
![Build Status badge](https://ersh.visualstudio.com/_apis/public/build/definitions/c9754d86-e84f-486e-a3b3-f7f42d31c01d/1/badge)

A tool to backup and catalog files.

Backups consist of file copies (backed up files) in `file_storage` directory and snapshot files in `snapshots`
directory. Snapshot file is a text file where every line describes one file: checksum, modification time, size, relative
path.

Example snapshot file `snapshots/MyBackup/2019-11-17_12-00-05`:

```
2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824 2019-11-17T11:56:53.2720115Z 5 hello.txt
a9a66978f378456c818fb8a3e7c6ad3d2c83e62724ccbdea7b36253fb8df5edd 2019-11-17T11:59:23.9767909Z 11 lorem/ipsum.txt
```

Files in file_storage are identified by their SHA-256 checksums. File storage contains no duplicates.

```
file_storage
+- a9
|  +- a9a66978f378456c818fb8a3e7c6ad3d2c83e62724ccbdea7b36253fb8df5edd
+- 2c
   +- 2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824
```

Backup command creates a snapshot file and then for every file in source folder it creates a copy in `file_storage` and
adds a line in snapshot file.

Restore command reads snapshot file, for every line it finds (by its checksum value) corresponding file in storage, and
creates a copy in the destination folder.

## Features
 - Calculates checksums. You can search files in catalog by checksum.
 - Supports hash functions: SHA-512, SHA-384, SHA-256, SHA-1, MD5, CRC32.
 - Works with long paths.
 - Catalogs files inside archives. Supports common archive types: Zip, 7Zip, Rar.

## Usage

    ctlg <command> [<options>]

### add

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

Show detailed information about catalog entry.

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

### pull-backup

    ctlg pull-backup -n <backup name> -d <date> <directory>

Imports backup from `<directory>`.

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
 - Autofac is used to inject dependencies.
 - SharpCompress is used to read archive files.
 - Crc32.NET is used to calculate CRC32 checksums.
 - NUnit and Moq for testing.
 - NLog is used for logging.
 - Command and Event patterns.

## Running on Mono

 - Download System.Data.SQLite full source [`sqlite-netFx-source`](https://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki).
 - Run `Setup/compile-interop-assembly-release.sh`.
 - Copy `bin/2013/Release/bin/libSQLite.Interop.dylib` to Ctlg application directory.

 [More info](https://stackoverflow.com/a/43173220/5642735).
