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

  # echo "# Setup ${CTLG_TEMPDIR}" >&3
}

teardown() {
  rm -rf "${CTLG_TEMPDIR}"
}
