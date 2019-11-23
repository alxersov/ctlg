#!/usr/bin/env bats

load helper

@test "pull-backup imports snapshot" {
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
  diff "$CTLG_WORKDIR/backup1/index.bin" "$CTLG_WORKDIR/backup2/index.bin"

  # running pull-backup second time should result in an error because destination shapshot file already exists
  run $CTLG_EXECUTABLE pull-backup -n Test "$CTLG_WORKDIR/backup1"
  [ "$status" -eq 2 ]
  [[ "$output" == *"File already exists"* ]] || false

  rm "$CTLG_WORKDIR/backup2/snapshots/Test/"*
  rm "$CTLG_WORKDIR/backup2/file_storage/2c/2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824"

  # when source snapshot does not exist
  run $CTLG_EXECUTABLE pull-backup -n Foo "$CTLG_WORKDIR/backup1"
  [ "$status" -eq 2 ]
  [[ "$output" == *"Snapshot Foo is not found in $CTLG_WORKDIR/backup1"* ]] || false

  # modifying file in storage, this will lead to a corrupted backup
  echo -n "test1" > "$CTLG_WORKDIR/backup1/file_storage/2c/2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824"
  run $CTLG_EXECUTABLE pull-backup -n Test "$CTLG_WORKDIR/backup1"
  [ "$status" -eq 2 ]
  [[ "$output" == *"Caclulated hash does not match expected"* ]] || false
}
