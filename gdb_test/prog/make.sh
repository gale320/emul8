#!/bin/bash
set -u
set -e

name="gdb_test"

arm-linux-gnueabi-gcc -g -nostdlib $name.c -o $name.elf
#arm-linux-gnueabi-objdump -D $name.elf > dump.txt
