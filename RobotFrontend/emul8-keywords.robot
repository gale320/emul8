*** Settings ***
Library         Process
Library         OperatingSystem
Library         emul8-lib.py

*** Variables ***
${PORT_NUMBER}      9999
${DIRECTORY}        ../output/Debug
${BINARY_NAME}      ./RobotFrontend.exe

*** Keywords ***
Setup
    Start Process                   ${BINARY_NAME}  ${PORT_NUMBER}  alias=Name  cwd=${DIRECTORY}
    Wait Until Keyword Succeeds     10s  1s  Import Library  Remote  http://localhost:${PORT_NUMBER}/

Teardown
    Quit
