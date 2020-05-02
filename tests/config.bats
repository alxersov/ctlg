#!/usr/bin/env bats

load helper

@test "backup and restore when hashing algorithm is specified" {
  echo '{"Version": 1, "HashAlgorithm": "SHA-512"}' > "$CTLG_WORKDIR/config.json"
  echo -n "hello" > "$CTLG_FILESDIR/hi.txt"
  output=$($CTLG_EXECUTABLE backup -n Test "$CTLG_FILESDIR")

  [[ "${output}" == *"1/1 HN 9b71d224      5 hi.txt"* ]] || false

  ${CTLG_EXECUTABLE} restore -n "Test" "${CTLG_RESTOREDIR}"

  diff -r "$CTLG_FILESDIR" "${CTLG_RESTOREDIR}"
}
