*** Settings ***
Suite Setup     Setup
Suite Teardown  Teardown
Resource        emul8-keywords.robot

*** Variables ***
${STM_BRIDGE}   STM32L151

*** Test Cases ***
Assa test
    ${x}=  Execute Command  help
    #Execute Script          assa
    #Start Emulation
    #Should Receive Message  ${STM_BRIDGE}  resetEvent${SPACE * 13}\r\nFrom\r\nAAAA0000000000000200000000000001\r\n  10000
    #Should Receive Message  ${STM_BRIDGE}  resetEvent${SPACE * 13}\r\nFrom\r\nAAAA0000000000000200000000000002\r\n  100000

