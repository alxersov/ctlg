﻿using System.IO;
using System.Security.Cryptography;

namespace Ctlg.Filesystem.Service
{
    public class HashService: IHashService
    {
        public HashService(SHA1 sha1)
        {
            Sha1 = sha1;
        }

        public byte[] CalculateSha1(Stream stream)
        {
            return Sha1.ComputeHash(stream);
        }

        private SHA1 Sha1 { get; }
    }
}