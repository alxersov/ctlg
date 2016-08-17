/*
Created: 06.08.2016
Modified: 16.08.2016
Model: Ctlg
Database: SQLite 3.7
*/

-- Create tables section -------------------------------------------------

-- Table File

CREATE TABLE File
(
  FileId INTEGER NOT NULL
        CONSTRAINT PK_File PRIMARY KEY AUTOINCREMENT,
  ParentFileId INTEGER,
  IsDirectory INTEGER NOT NULL,
  Name TEXT NOT NULL,
  Size INTEGER,
  FileCreatedDateTime INTEGER,
  FileModifiedDateTime INTEGER,
  RecordUpdatedDateTime INTEGER NOT NULL,
  CONSTRAINT FK_File_ParentFile FOREIGN KEY (ParentFileId) REFERENCES File (FileId)
);

CREATE INDEX IX_File_1 ON File (ParentFileId);

-- Table Hash

CREATE TABLE Hash
(
  HashId INTEGER NOT NULL
        CONSTRAINT Key1 PRIMARY KEY AUTOINCREMENT,
  HashAlgorithmId INTEGER NOT NULL,
  Value blob NOT NULL,
  CONSTRAINT FK_Hash_HashAlgorithm FOREIGN KEY (HashAlgorithmId) REFERENCES HashAlgorithm (HashAlgorithmId)
);

CREATE UNIQUE INDEX IX_Hash_1 ON Hash (HashAlgorithmId,Value);

-- Table FileHash

CREATE TABLE FileHash
(
  FileId INTEGER NOT NULL,
  HashId INTEGER NOT NULL,
  CONSTRAINT PK_FileHash PRIMARY KEY (FileId,HashId),
  CONSTRAINT FK_File_FileHash FOREIGN KEY (FileId) REFERENCES File (FileId),
  CONSTRAINT FK_Hash_FileHash FOREIGN KEY (HashId) REFERENCES Hash (HashId)
);

-- Table HashAlgorithm

CREATE TABLE HashAlgorithm
(
  HashAlgorithmId INTEGER NOT NULL,
  Name TEXT NOT NULL,
  CONSTRAINT PK_HashAlgorithm PRIMARY KEY (HashAlgorithmId)
);


