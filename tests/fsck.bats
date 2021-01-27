#!/usr/bin/env bats

load helper

@test "detects corrupted files in storage" {
  echo -n "hi" > "$CTLG_FILESDIR/hi.txt"
  echo -n "hello" > "$CTLG_FILESDIR/hello.txt"
  $CTLG_EXECUTABLE backup -n Test "$CTLG_FILESDIR"

  echo -n "AAAAA" > "$CTLG_WORKDIR/file_storage/2c/2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824"

  run $CTLG_EXECUTABLE fsck
  [ "$status" -eq 2 ]
  [[ "$output" == *"expected: 2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824 got 11770b3ea657fe68cba19675143e4715c8de9d763d3c21a85af6b7513d43997d"* ]] || false
}

@test "detects missing files in storage" {
  echo -n "hi" > "$CTLG_FILESDIR/hi.txt"
  echo -n "hello" > "$CTLG_FILESDIR/hello.txt"
  $CTLG_EXECUTABLE backup -n Test "$CTLG_FILESDIR"

  rm "$CTLG_WORKDIR/file_storage/2c/2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824"

  run $CTLG_EXECUTABLE fsck
  [ "$status" -eq 2 ]
  [[ "$output" == *"File hello.txt is not found"* ]] || false
}

@test "detects wrong file size in snapshots" {
  echo -n "hi" > "$CTLG_FILESDIR/hi.txt"
  echo -n "hello" > "$CTLG_FILESDIR/hello.txt"
  $CTLG_EXECUTABLE backup -n Test "$CTLG_FILESDIR"

  echo -n "2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824 2021-01-25T17:00:00.0000000Z 10 hello.txt" > "$CTLG_WORKDIR/snapshots/Test/2021-01-25_18-00-00"

  run $CTLG_EXECUTABLE fsck
  echo "$output"
  [ "$status" -eq 2 ]
  regex="The size of \"hello.txt\" and \".*2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824\" do not match"
  [[ "$output" =~ $regex ]] || false
}

@test "outputs progress" {
  echo -n "hi" > "$CTLG_FILESDIR/hi.txt"
  echo -n "hello" > "$CTLG_FILESDIR/hello.txt"
  $CTLG_EXECUTABLE backup -n Test "$CTLG_FILESDIR"

  run $CTLG_EXECUTABLE fsck
  echo "$output"
  [ "$status" -eq 0 ]
  [[ "$output" == *"Enumerating files in storage with 8f"* ]] || false
  [[ "$output" == *"Enumerating files in storage with 2c"* ]] || false
  [[ "$output" == *"Processing snapshot Test @ $(ls snapshots/Test | tail -1)"* ]] || false
}
