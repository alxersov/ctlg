ALTER TABLE HashAlgorithm ADD Length INTEGER;

UPDATE HashAlgorithm SET Length = 20 WHERE Name = 'SHA-1';
UPDATE HashAlgorithm SET Length = 32 WHERE Name = 'SHA-256';
UPDATE HashAlgorithm SET Length = 48 WHERE Name = 'SHA-384';
UPDATE HashAlgorithm SET Length = 64 WHERE Name = 'SHA-512';
UPDATE HashAlgorithm SET Length = 16 WHERE Name = 'MD5';
UPDATE HashAlgorithm SET Length = 4 WHERE Name = 'CRC32';
