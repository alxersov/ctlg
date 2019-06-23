#!/usr/bin/env bats

setup() {
  CTLG_TEMPDIR="$(mktemp -d "${BATS_TMPDIR}/ctlg_tests.XXX")"
  CTLG_FILESDIR="${CTLG_TEMPDIR}/files"
  CTLG_RESTOREDIR="${CTLG_TEMPDIR}/restore"
  CTLG_WORKDIR="${CTLG_TEMPDIR}/work"
  CTLG_EXECUTABLE="mono ${BATS_TEST_DIRNAME}/../Ctlg/bin/Debug/Ctlg.exe"


  mkdir "${CTLG_FILESDIR}"
  mkdir "${CTLG_WORKDIR}"
  mkdir "${CTLG_RESTOREDIR}"
  cd "${CTLG_WORKDIR}"

  echo "# Setup ${CTLG_TEMPDIR}" >&3
}

teardown() {
  rm -rf "${CTLG_TEMPDIR}"
  :
}

@test "backup and restore one file" {
  echo -n "hello" > ${CTLG_FILESDIR}/hi.txt
  output=$($CTLG_EXECUTABLE backup -n Test ${CTLG_FILESDIR})

  [[ "${output}" == *"1/1 HN 2cf24dba      5 hi.txt"* ]]

  ${CTLG_EXECUTABLE} restore -n "snapshots/Test/$(ls snapshots/Test | tail -1)" "${CTLG_RESTOREDIR}"

  diff -r "$CTLG_FILESDIR" "${CTLG_RESTOREDIR}"
}

@test "backup and restore multiple files and directories" {
  echo -n "hello" > "$CTLG_FILESDIR/hi.txt"
  mkdir "$CTLG_FILESDIR/foo"
  mkdir "$CTLG_FILESDIR/foo/b r"
  echo -n "world" > "$CTLG_FILESDIR/foo/b r/w.txt"

  $CTLG_EXECUTABLE backup -n Test ${CTLG_FILESDIR}
  ${CTLG_EXECUTABLE} restore -n "snapshots/Test/$(ls snapshots/Test | tail -1)" "${CTLG_RESTOREDIR}"
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

  ${CTLG_EXECUTABLE} restore -n "snapshots/Test2/$(ls snapshots/Test2 | tail -1)" "${CTLG_RESTOREDIR}"
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
