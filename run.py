import time
import time
import pygame
import keyboard
mem={}
addresses={}
pc=0
def run_alloc(arg1,arg2=0):
    global mem
    if str(arg1)[0]=="@":
        arg1=arg1.replace("@","")
    mem[arg1]=arg2
def run_exit():
    global pc
    pc=-2
def run_print(arg):
    print(arg)
def run_greater(arg1,arg2,arg3):
    if str(arg1)[0]=="@":
        arg1=mem[arg1.replace("@","")]
    if str(arg2)[0]=="@":
        arg2=mem[arg2.replace("@","")]
    if arg1>arg2:
        if type(arg3)==type(3):
            run_jump(arg3)
        else:
            run_address(arg3)
def run_key(arg1,arg2):
    if keyboard.is_pressed(arg1):
        mem[arg2.replace("@","")]=mem[arg2.replace("@","")]+1
def run_address(arg):
    global pc
    global addresses
    if addresses.get(arg)!={}.get("a"):
        pc=addresses[arg]
    else:
        addresses[arg]=pc
def run_inc(arg):
    if str(arg)[0]=="@":
        arg=arg.replace("@","")
    global mem
    mem[arg]=mem[arg]+1
def run_dec(arg):
    global mem
    mem[arg]=mem[arg]-1
def run_pixel(x,y,r,g,b):
    global draw
    if draw:
        if str(x)[0]=="@":
            x=mem[x.replace("@","")]
        if str(y)[0]=="@":
            y=mem[y.replace("@","")]
        if str(r)[0]=="@":
            r=mem[r.replace("@","")]
        if str(g)[0]=="@":
            g=mem[g.replace("@","")]
        if str(b)[0]=="@":
            b=mem[b.replace("@","")]
        pygame.Surface.set_at(screen,(x,y),(r,g,b))
        if not useBuffer:
            pygame.display.flip()
def run_buffer():
    pygame.display.flip()
def run_jump(arg):
    global pc
    if str(arg)[0]=="@":
        arg=mem[arg.replace("@","")]
    pc=arg-2
def run_window(arg,width=318,height=212):
    if str(arg)[0]=="@":
        arg=mem[arg.replace("@","")]
    if str(width)[0]=="@":
        width=mem[width.replace("@","")]
    if str(height)[0]=="@":
        height=mem[height.replace("@","")]
    global draw
    global useBuffer
    global screen
    draw = True
    useBuffer = bool(arg)
    pygame.init()
    screen=pygame.display.set_mode((width,height))
def run_time(arg):
    if str(arg)[0]=="@":
        arg=arg.replace("@","")
    global mem
    mem[arg]=time.time()
def parse_number(s):
    try:
        i = int(s)
        return i
    except ValueError:
        try:
            f = float(s)
            return f
        except ValueError:
            return None
def run(stack):
    global mem
    global addresses
    global pc
    buffer=""
    for i in range(len(stack)):
        buffer=buffer+stack[i]
    stack=str(buffer)
    com = False
    buffer=""
    for i in range(0,len(stack)):
        if stack[i]=='"':
            com = not com
        elif (stack[i]!=" " or com)and(stack[i]!="$"):
            buffer=buffer+stack[i]
        elif stack[i]==" ":
            buffer=buffer+","
    buffer=buffer.split(";")
    buffer.pop()
    for i in range(len(buffer)):
        buffer[i]=buffer[i].split(",")
        for x in range(0,len(buffer[i])):
            if parse_number(buffer[i][x])!=None:
                buffer[i][x]=parse_number(buffer[i][x])
    stack=buffer
    pc=0
    draw=False
    useBuffer=False
    mem={}
    addresses={}
    for i in range(len(stack)):
        if stack[i][0]=="ADDRESS":
            pc=i
            run_address(stack[i][1])
    pc=0
    while pc!=-1 and pc<len(stack):
        if stack[pc][0]=="EXIT":
            run_exit()
        elif stack[pc][0]=="ALLOC":
            if len(stack[pc])==2:
                run_alloc(stack[pc][1])
            else:
                run_alloc(stack[pc][1],stack[pc][2])
        elif stack[pc][0]=="PRINT":
            arg=""
            for i in range(1,len(stack[pc])):
                if str(stack[pc][i])[0]=="@":
                    arg=arg+str(mem[stack[pc][i].replace("@","")])
                else:
                    arg=arg+str(stack[pc][i])
            run_print(arg)
        elif stack[pc][0]=="KEY":
            run_key(stack[pc][1],stack[pc][2])
        elif stack[pc][0]=="GREATER":
            run_greater(stack[pc][1],stack[pc][2],stack[pc][3])
        elif stack[pc][0]=="JUMP":
            run_jump(stack[pc][1])
        elif stack[pc][0]=="INC":
            run_inc(stack[pc][1])
        elif stack[pc][0]=="DEC":
            run_dec(stack[pc][1])
        elif stack[pc][0]=="WINDOW":
            if (len(stack[pc])==2):
                run_window(stack[pc][1])
            else:
                run_window(stack[pc][1],stack[pc][2],stack[pc][3])
        elif stack[pc][0]=="PIXEL":
            run_pixel(stack[pc][1],stack[pc][2],stack[pc][3],stack[pc][4],stack[pc][5])
        elif stack[pc][0]=="BUFFER":
            run_buffer()
        elif stack[pc][0]=="ADDRESS":
            run_address(stack[pc][1])
        elif stack[pc][0]=="TIME":
            run_time(stack[pc][1])
        pc=pc+1
import sys 

#This is an old version that is only here for backwards compatability
def version1(program):
    import re
    import keyboard
    mem = {}
    if True:
        program = re.sub(r'/.*?/', '', program)
        program.replace(".","")
        list = program.replace("\n","").split(";")
        PC = 0
        while PC!=-1 and PC<len(list):
            if (list[PC].startswith("PRINT")):
                string=list[PC].replace("PRINT ","")
                string = string.replace(";","")
                args = string.split(",")
                for c in range(len(args)):
                    if (args[c].startswith("$")):
                        args[c] = args[c].replace("\"","")
                        args[c] = args[c].replace("$","")
                        print(args[c])
                    elif (args[c].startswith("@")):
                        args[c] = args[c].replace("@","")
                        print(mem[args[c]])
                    else:
                        print(int(args[c]))
            elif (list[PC].startswith("LINE")):
                intVal = list[PC].replace("LINE ","")
                intVal = intVal.replace(";","")
                if (intVal.startswith("@")):
                    intVal = intVal.replace("@","")
                    intVal = mem[intVal]
                intVal = int(intVal)
                PC = intVal-2
            elif (list[PC].startswith("EXIT")):
                PC = -2
            elif (list[PC].startswith("INT")):
                name = list[PC].replace("INT ","")
                name = name.replace(";","")
                name = name.split(",")[0]
                name = name.replace("@","")
                if (list[PC].count(",")>1):
                    mem[name] = int(list[PC].split(",")[1].replace(";",""))
                else:
                    mem[name] = 0
            elif (list[PC].startswith("INC")):
                name = list[PC].replace("INC ","")
                name = name.replace(";","")
                name = name.replace("@","")
                mem[name] = mem[name]+1
            elif (list[PC].startswith("DEC")):
                name = list[PC].replace("DEC ","")
                name = name.replace(";","")
                name = name.replace("@","")
                mem[name] = mem[name]-1
            elif (list[PC].startswith("KEY")):
                name = list[PC].replace("KEY ","")
                name = name.replace(";","")
                arg1 = name.split(",")[0].replace("$","")
                arg1 = arg1.replace("\"","")
                arg2 = name.split(",")[1].replace("@","")
                if (keyboard.is_pressed(arg1)):
                    mem[arg2] = mem[arg2] + 1
            elif (list[PC].startswith("SET ")):
                args = list[PC].replace("SET ","")
                args = args.replace(";","")
                args = args.split(",")
                args[0] = args[0].replace("@","")
                args[0] = args[0].replace(" ","")
                value = 0
                if (len(args)>1):
                    if (args[1].startswith("@")):
                        value = mem[args[1].replace("@","")]
                    elif (args[1].isdigit()):
                        value = int(args[1])
                mem[args[0]] = value
            elif (list[PC].startswith("GREATER")):
                args = list[PC].replace("GREATER ","")
                args = args.replace(";","")
                args = args.replace(" ","")
                args = args.split(",")
                if (args[0].startswith("@")):
                    arg1 = mem[args[0].replace("@","")]
                elif (args[0].isdigit()):
                    arg1 = int(args[0])
                if (args[1].startswith("@")):
                    arg2 = mem[args[1].replace("@","")]
                elif (args[1].isdigit()):
                    arg2 = int(args[1])
                if (args[2].startswith("@")):
                    arg3 = mem[args[2].replace("@","")]
                elif (args[2].isdigit()):
                    arg3 = int(args[2])
                if (arg1>arg2):
                    PC = arg3-2
            elif (list[PC].startswith("PIXEL")):
                args = list[PC].replace("PIXEL ","")
                args = args.replace(";","")
                args = args.replace(" ","")
                args = args.split(",")
                if (args[0].startswith("@")):
                    arg1 = mem[args[0].replace("@","")]
                elif (args[0].isdigit()):
                    arg1 = int(args[0])
                if (args[1].startswith("@")):
                    arg2 = mem[args[1].replace("@","")]
                elif (args[1].isdigit()):
                    arg2 = int(args[1])
                if (args[2].startswith("@")):
                    arg3 = mem[args[2].replace("@","")]
                elif (args[2].isdigit()):
                    arg3 = int(args[2])
                if (args[3].startswith("@")):
                    arg4 = mem[args[3].replace("@","")]
                elif (args[3].isdigit()):
                    arg4 = int(args[3])
                if (args[4].startswith("@")):
                    arg5 = mem[args[4].replace("@","")]
                elif (args[4].isdigit()):
                    arg5 = int(args[4])
                pygame.Surface.set_at(screen,(arg1,arg2),(arg3,arg4,arg5))
                pygame.display.flip()
            elif (list[PC].startswith("WINDOW")):
                import pygame
                pygame.init()
                screen=pygame.display.set_mode((318,212))
            PC = PC+1
def version2(string):
    import re
    string = re.sub(r'/.*?/', '', string)
    string = string.replace("\n","")
    string = string.split(";")
    string.pop()
    for i in range(len(string)):
        string[i]=string[i]+";"
    run(string)
with open(sys.argv[1]) as f:
    string = f.read()
    string = string.replace("\n\n","\n")
    if (string.startswith("VERSION 0.1.0;\n")):
        string = string.replace("VERSION 0.1.0;\n","")
        version1(string)
    elif (string.startswith("VERSION 0.2.0;\n")):
        string = string.replace("VERSION 0.2.0;\n","")
        version2(string)
