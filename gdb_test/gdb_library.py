#!/usr/bin/python
import select
import subprocess

gdb_binary = './arm-none-eabi-gdb'
gdb_process = None

def start_gdb():
    global gdb_process
    gdb_process = subprocess.Popen([gdb_binary, '-q'], stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    gdb_process.stdin.write('set prompt (gdb)\\n \n')
    gdb_process.stdout.readline()

def command_gdb(cmd):
    result = ''
    current_line = ''
    gdb_process.stdin.write(cmd + '\n')
    poll_obj = select.poll()
    poll_obj.register(gdb_process.stdout, select.POLLIN)
    poll_obj.register(gdb_process.stderr, select.POLLIN)

    while(True):
        poll_results = poll_obj.poll(1000)
        for poll_result in poll_results:
            fd = poll_result[0]
            if fd == gdb_process.stdout.fileno():
                c = gdb_process.stdout.read(1)
                if c == '\n':
                    if current_line.strip() == '(gdb)':
                        return result
                    else:
                        result += current_line + '\n'
                        current_line = ''
                else:
                    current_line += c
            elif fd == gdb_process.stderr.fileno():
                raise Exception(gdb_process.stderr.readline())

def stop_gdb():
    gdb_process.terminate()

if __name__ == '__main__':
    start_gdb()
