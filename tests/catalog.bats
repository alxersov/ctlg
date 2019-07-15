#!/usr/bin/env bats

load helper

@test "add one file using default checksum method" {
  echo -n "hello" > ${CTLG_FILESDIR}/hi.txt
  $CTLG_EXECUTABLE add ${CTLG_FILESDIR}

  output=$($CTLG_EXECUTABLE find -c 2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824 -f sha-256)

  [[ "${output}" == *"hi.txt"* ]]
}
