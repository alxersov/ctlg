#!/usr/bin/env bats

load helper

@test "backup-pull imports snapshot" {
  echo -n "hello" > "$CTLG_FILESDIR/hi.txt"
  mkdir "$CTLG_FILESDIR/foo"
  echo -n "world" > "$CTLG_FILESDIR/foo/w.txt"

  mkdir "$CTLG_WORKDIR/backup1"
  cd "$CTLG_WORKDIR/backup1"

  $CTLG_EXECUTABLE backup -n Test ${CTLG_FILESDIR}

  mkdir "$CTLG_WORKDIR/backup2"
  cd "$CTLG_WORKDIR/backup2"

  $CTLG_EXECUTABLE pull-backup -n Test "$CTLG_WORKDIR/backup1"

  $CTLG_EXECUTABLE restore -n Test "$CTLG_RESTOREDIR"
  diff -r "$CTLG_FILESDIR" "$CTLG_RESTOREDIR"
}
