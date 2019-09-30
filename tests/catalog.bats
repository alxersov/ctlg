#!/usr/bin/env bats

load helper

@test "add one file using default checksum method" {
  echo -n "hello" > $CTLG_FILESDIR/hi.txt
  $CTLG_EXECUTABLE add $CTLG_FILESDIR

  output=$($CTLG_EXECUTABLE find -c 2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824 -f sha-256)
  [[ "${output}" == *"hi.txt"* ]] || false

  output=$($CTLG_EXECUTABLE list)
  [[ "${output}" == *"hi.txt"* ]] || false

  output=$($CTLG_EXECUTABLE show 2)
  [[ "${output}" == *"hi.txt"* ]] || false
}

@test "when adding archive it parses content" {
  echo -n "hello" > $CTLG_FILESDIR/hi.txt
  zip -j $CTLG_FILESDIR/1.zip $CTLG_FILESDIR/hi.txt
  rm $CTLG_FILESDIR/hi.txt
  $CTLG_EXECUTABLE add $CTLG_FILESDIR

  output=$($CTLG_EXECUTABLE find --name "hi.txt")
  [[ "${output}" == *"hi.txt"* ]] || false

  output=$($CTLG_EXECUTABLE find --checksum 3610a686 --function CRC32)
  [[ "${output}" == *"hi.txt"* ]] || false

  output=$($CTLG_EXECUTABLE find --size 5)
  [[ "${output}" == *"hi.txt"* ]] || false
}

@test "add files filtering by name" {
  echo -n "hello" > $CTLG_FILESDIR/hi.txt
  echo -n "world" > $CTLG_FILESDIR/w.txt
  echo -n "test" > $CTLG_FILESDIR/test

  output=$($CTLG_EXECUTABLE add --search "*.txt" --function CRC32 $CTLG_FILESDIR)
  [[ "${output}" == *"2 files found"* ]] || false
}
