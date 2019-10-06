#!/usr/bin/env bats

load helper

@test "index recreate" {
  echo -n "hi" > "$CTLG_FILESDIR/hi.txt"
  echo -n "hello" > "$CTLG_FILESDIR/hello.txt"
  $CTLG_EXECUTABLE backup -n Test ${CTLG_FILESDIR}

  sum1="\x2c\xf2\x4d\xba\x5f\xb0\xa3\x0e\x26\xe8\x3b\x2a\xc5\xb9\xe2\x9e\x1b\x16\x1e\x5c\x1f\xa7\x42\x5e\x73\x04\x33\x62\x93\x8b\x98\x24"
  sum2="\x8f\x43\x43\x46\x64\x8f\x6b\x96\xdf\x89\xdd\xa9\x01\xc5\x17\x6b\x10\xa6\xd8\x39\x61\xdd\x3c\x1a\xc8\x8b\x59\xb2\xdc\x32\x7a\xa4"

  echo -n -e "$sum1$sum2" > "${CTLG_FILESDIR}/expected-index.bin"

  ${CTLG_EXECUTABLE} rebuild-index

  diff "$CTLG_WORKDIR/index.bin" "${CTLG_FILESDIR}/expected-index.bin"
}
