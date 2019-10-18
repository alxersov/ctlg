#!/usr/bin/env bats

load helper

@test "backup and restore one file" {
  echo -n "hello" > ${CTLG_FILESDIR}/hi.txt
  output=$($CTLG_EXECUTABLE backup -n Test ${CTLG_FILESDIR})

  [[ "${output}" == *"1/1 HN 2cf24dba      5 hi.txt"* ]] || false
  [[ "${output}" == *"Processed: 5"* ]] || false
  [[ "${output}" == *"Added to storage: 5"* ]] || false

  ${CTLG_EXECUTABLE} restore -n "Test" "${CTLG_RESTOREDIR}"

  diff -r "$CTLG_FILESDIR" "${CTLG_RESTOREDIR}"
}

@test "backup and restore one file with fast mode" {
  echo -n "hello" > ${CTLG_FILESDIR}/hi.txt
  output=$($CTLG_EXECUTABLE backup -f -n Test ${CTLG_FILESDIR})

  [[ "${output}" == *"1/1 HN 2cf24dba      5 hi.txt"* ]] || false

  ${CTLG_EXECUTABLE} restore -n "Test" "${CTLG_RESTOREDIR}"

  diff -r "$CTLG_FILESDIR" "${CTLG_RESTOREDIR}"
}

@test "backup in fast mode works when files are deleted" {
  echo -n "hello" > "$CTLG_FILESDIR/hi.txt"
  $CTLG_EXECUTABLE backup -n Test "$CTLG_FILESDIR"

  rm "$CTLG_FILESDIR/hi.txt"

  $CTLG_EXECUTABLE backup -f -n Test "$CTLG_FILESDIR"
}

@test "backup and restore multiple files and directories" {
  echo -n "hello" > "$CTLG_FILESDIR/hi.txt"
  mkdir "$CTLG_FILESDIR/foo"
  mkdir "$CTLG_FILESDIR/foo/b r"
  echo -n "world" > "$CTLG_FILESDIR/foo/b r/w.txt"

  $CTLG_EXECUTABLE backup -n Test ${CTLG_FILESDIR}
  ${CTLG_EXECUTABLE} restore -n Test "${CTLG_RESTOREDIR}"
  diff -r "$CTLG_FILESDIR" "${CTLG_RESTOREDIR}"
}

@test "restore one of multiple backups" {
  echo -n "hello" > "$CTLG_FILESDIR/hi.txt"
  $CTLG_EXECUTABLE backup -n Test1 ${CTLG_FILESDIR}
  echo -n "world" > "$CTLG_FILESDIR/world.txt"
  $CTLG_EXECUTABLE backup -n Test2 ${CTLG_FILESDIR}
  rm "$CTLG_FILESDIR/world.txt"
  echo -n "TEST" > "$CTLG_FILESDIR/hi.txt"
  $CTLG_EXECUTABLE backup -n Test1 ${CTLG_FILESDIR}

  ${CTLG_EXECUTABLE} restore -n "Test2" "${CTLG_RESTOREDIR}"
  echo -n "hello" > "$CTLG_FILESDIR/hi.txt"
  echo -n "world" > "$CTLG_FILESDIR/world.txt"
  diff -r "$CTLG_FILESDIR" "${CTLG_RESTOREDIR}"
}

@test "backup filters files by search pattern" {
  echo -n "hello" > "$CTLG_FILESDIR/hi.txt"
  echo -n "test" > "$CTLG_FILESDIR/hi.bin"

  $CTLG_EXECUTABLE backup -n Test1 -s *.txt ${CTLG_FILESDIR}

  snapshot="snapshots/Test1/$(ls snapshots/Test1 | tail -1)"
  file_list=$(cat "$snapshot" | awk '{ print $4 }')
  [ "$file_list" == "hi.txt" ]
}

@test "restore backup for differnt dates" {
  echo -n "1" > "$CTLG_FILESDIR/hi.txt"
  $CTLG_EXECUTABLE backup -n Test $CTLG_FILESDIR
  mv "snapshots/Test/$(ls snapshots/Test | tail -1)" "snapshots/Test/2019-01-01_09-10-15"

  echo -n "2" > "$CTLG_FILESDIR/hi.txt"
  $CTLG_EXECUTABLE backup -n Test $CTLG_FILESDIR
  mv "snapshots/Test/$(ls snapshots/Test | tail -1)" "snapshots/Test/2019-01-02_03-04-10"

  echo -n "3" > "$CTLG_FILESDIR/hi.txt"
  $CTLG_EXECUTABLE backup -n Test $CTLG_FILESDIR
  mv "snapshots/Test/$(ls snapshots/Test | tail -1)" "snapshots/Test/2019-01-02_03-04-15"

  $CTLG_EXECUTABLE restore -n "Test" "$CTLG_RESTOREDIR"
  grep -Fxq "3" "$CTLG_RESTOREDIR/hi.txt"
  rm "$CTLG_RESTOREDIR/hi.txt"

  $CTLG_EXECUTABLE restore -n "Test" -d "2019-01-02_03-04-10" "$CTLG_RESTOREDIR"
  grep -Fxq "2" "$CTLG_RESTOREDIR/hi.txt"
  rm "$CTLG_RESTOREDIR/hi.txt"

  $CTLG_EXECUTABLE restore -n "Test" -d "2019-01-01" "$CTLG_RESTOREDIR"
  grep -Fxq "1" "$CTLG_RESTOREDIR/hi.txt"
  rm "$CTLG_RESTOREDIR/hi.txt"

  run $CTLG_EXECUTABLE restore -n "Test" -d "2019-01-02" "$CTLG_RESTOREDIR"
  [ "$status" -eq 2 ]
  [[ "$output" == *"snapshot date is ambiguous"* ]] || false

  run $CTLG_EXECUTABLE restore -n "Test" -d "2019-01-03" "$CTLG_RESTOREDIR"
  [ "$status" -eq 2 ]
  [[ "$output" == *"Snapshot Test is not found"* ]] || false

  run $CTLG_EXECUTABLE restore -n "DoesNotExist" -d "2019-01-01" "$CTLG_RESTOREDIR"
  [ "$status" -eq 2 ]
  [[ "$output" == *"Snapshot DoesNotExist is not found"* ]] || false
}
