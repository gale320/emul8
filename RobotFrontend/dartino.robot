*** Settings ***
Suite Setup     Setup  ${dir}
Suite Teardown  Teardown
Test Teardown   Clear Emulation
Resource        emul8-keywords.robot

*** Keywords ***

Clear Emulation
        Execute Command  Clear

Execute Dartino Test
    [Arguments]         ${binary}
            Execute Command     createPlatform STM32F746
            Execute Command     sysbus LoadELF @${binary}
            Add Breakpoint      StartDartino
            Start Emulation
            Wait Until Keyword Succeeds     5 sec   1 sec   Machine Should Be Paused
    ${x}=   Find Current Symbol
            Should Contain      ${x}    StartDartino
            Add Breakpoint      dartino::Scheduler::HandleTerminated
            Add Breakpoint      dartino::Scheduler::HandleUncaughtException
            Add Breakpoint      dartino::Scheduler::HandleCompileTimeError
            Continue From Breakpoint
            Wait Until Keyword Succeeds     5 sec   1 sec   Machine Should Be Paused
    ${x}=   Find Current Symbol
            Log To Console      ${x}

*** Test Cases ***

Should Run A Test
    Execute Dartino Test   ${binary}

