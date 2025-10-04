import re,time,os,keyboard,json,math,ctypes,sys
if getattr(sys, 'frozen', False):
    base_path = os.path.dirname(os.path.dirname(sys.executable))
else:
    base_path = os.path.dirname(os.path.abspath(__file__))
os.environ["PYSDL2_DLL_PATH"] = base_path
import sdl2,sdl2.ext
os.system("cls")
start = time.time()
numbers={}
addresses={}
strings={}
functions={}
images = {}
tooltip_descriptions={}
tooltip_arguments={}
JSON = ""
pc=0
comment = []
stack = []
def pack_color(r, g, b, a=255):
    return (a << 24) | (b << 16) | (g << 8) | r
def run_alloc(arg1,arg2=0):
    global numbers
    if str(arg2)[0]=="@":
        numbers[arg1]=numbers[arg2]
    else:
        numbers[arg1]=arg2
def run_exit():
    global pc
    pc=-2
def run_print(arg):
    print(arg)
def run_greater(arg1,arg2,arg3):
    if str(arg1)[0]=="@":
        arg1=numbers[arg1]
    if str(arg2)[0]=="@":
        arg2=numbers[arg2]
    if arg1>arg2:
        if type(arg3)==type(3):
            run_jump(arg3)
        else:
            run_address(arg3)
def run_key(arg1,arg2):
    if keyboard.is_pressed(arg1):
        numbers[arg2]=1
    else:
        numbers[arg2]=0
def run_address(arg):
    global pc
    global addresses
    if addresses.get(arg)!={}.get("a"):
        pc=addresses[arg]
    else:
        addresses[arg]=pc
def run_inc(arg):
    global numbers
    numbers[arg]=numbers[arg]+1
def run_dec(arg):
    global numbers
    numbers[arg]=numbers[arg]-1
def run_pixel(x,y,r,g,b):
    global draw
    global px
    if draw:
        if str(x)[0]=="@":
            x=numbers[x]
        if str(y)[0]=="@":
            y=numbers[y]
        if str(r)[0]=="@":
            r=numbers[r]
        if str(g)[0]=="@":
            g=numbers[g]
        if str(b)[0]=="@":
            b=numbers[b]
        sdl2.SDL_SetRenderDrawColor(renderer, r, g, b, 255)
        sdl2.SDL_RenderDrawPoint(renderer, x, y)
        if not useBuffer:
            sdl2.SDL_RenderPresent(renderer)
def run_rect(x,y,w,h,r,g,b):
    global draw
    global px
    if draw:
        if str(x)[0]=="@":
            x=numbers[x]
        if str(y)[0]=="@":
            y=numbers[y]
        if str(w)[0]=="@":
            w=numbers[w]
        if str(h)[0]=="@":
            h=numbers[h]
        if str(r)[0]=="@":
            r=numbers[r]
        if str(g)[0]=="@":
            g=numbers[g]
        if str(b)[0]=="@":
            b=numbers[b]
        sdl2.SDL_SetRenderDrawColor(renderer, int(r), int(g), int(b), 255)
        rect = sdl2.SDL_Rect(x, y, w, h)
        sdl2.SDL_RenderFillRect(renderer, rect)
        if not useBuffer:
            sdl2.SDL_RenderPresent(renderer)
def run_gradient(x,y,w,h,r1,g1,b1,r2,g2,b2,vert):
    global draw
    if draw:
        if str(x)[0]=="@":
            x=numbers[x]
        if str(y)[0]=="@":
            y=numbers[y]
        if str(w)[0]=="@":
            w=numbers[w]
        if str(h)[0]=="@":
            h=numbers[h]
        if str(r1)[0]=="@":
            r1=numbers[r1]
        if str(g1)[0]=="@":
            g1=numbers[g1]
        if str(b1)[0]=="@":
            b1=numbers[b1]
        if str(r2)[0]=="@":
            r2=numbers[r2]
        if str(g2)[0]=="@":
            g2=numbers[g2]
        if str(b2)[0]=="@":
            b2=numbers[b2]
        tex_surface = sdl2.SDL_CreateRGBSurfaceWithFormat(0, 2, 2, 32, sdl2.SDL_PIXELFORMAT_RGBA32)
        sdl2.SDL_LockSurface(tex_surface)
        pixels = ctypes.cast(tex_surface.contents.pixels, ctypes.POINTER(ctypes.c_uint32))
        if vert:
            pixels[0] = pack_color(r1, g1, b1)
            pixels[1] = pack_color(r1, g1, b1)
            pixels[2] = pack_color(r2, g2, b2)
            pixels[3] = pack_color(r2, g2, b2)
        else:
            pixels[0] = pack_color(r1, g1, b1)
            pixels[2] = pack_color(r1, g1, b1)
            pixels[1] = pack_color(r2, g2, b2)
            pixels[3] = pack_color(r2, g2, b2)
        sdl2.SDL_UnlockSurface(tex_surface)
        gradient_tex = sdl2.SDL_CreateTextureFromSurface(renderer, tex_surface)
        dst_rect = sdl2.SDL_Rect(x, y, w, h)
        sdl2.SDL_RenderCopy(renderer, gradient_tex, None, dst_rect)
        sdl2.SDL_DestroyTexture(gradient_tex)
        if not useBuffer:
            sdl2.SDL_RenderPresent(renderer)
def run_trig(x1,y1,x2,y2,x3,y3,r,g,b):
    global draw
    if draw:
        if str(x1)[0]=="@":
            x1=numbers[x1]
        if str(y1)[0]=="@":
            y1=numbers[y1]
        if str(x2)[0]=="@":
            x2=numbers[x2]
        if str(y2)[0]=="@":
            y2=numbers[y2]
        if str(x3)[0]=="@":
            x3=numbers[x3]
        if str(y3)[0]=="@":
            y3=numbers[y3]
        if str(r)[0]=="@":
            r=numbers[r]
        if str(g)[0]=="@":
            g=numbers[g]
        if str(b)[0]=="@":
            b=numbers[b]
        color = sdl2.SDL_Color(int(r), int(g), int(b), 255)
        vertices = (sdl2.SDL_Vertex * 3)(sdl2.SDL_Vertex(sdl2.SDL_FPoint(x1, y1), color, sdl2.SDL_FPoint(0, 0)),sdl2.SDL_Vertex(sdl2.SDL_FPoint(x2, y2), color, sdl2.SDL_FPoint(0, 0)),sdl2.SDL_Vertex(sdl2.SDL_FPoint(x3, y3), color, sdl2.SDL_FPoint(0, 0)),)
        indices = (ctypes.c_int * 3)(0, 1, 2)
        sdl2.SDL_RenderGeometry(renderer, None, vertices, 3, indices, 3)
        if not useBuffer:
            sdl2.SDL_RenderPresent(renderer)
def run_buffer():
    sdl2.SDL_RenderPresent(renderer)
def run_jump(arg):
    global pc
    if str(arg)[0]=="@":
        arg=numbers[arg]
    pc=arg-2
def run_window(arg,width=318,height=212):
    global renderer
    global window
    global draw
    global useBuffer
    if str(arg)[0]=="@":
        arg=numbers[arg]
    if str(width)[0]=="@":
        width=numbers[width]
    if str(height)[0]=="@":
        height=numbers[height]
    draw = True
    useBuffer = bool(arg)
    sdl2.ext.init()
    window = sdl2.ext.Window("Redmond Program", size=(width,height))
    renderer = sdl2.SDL_CreateRenderer(window.window,-1,sdl2.SDL_RENDERER_ACCELERATED | sdl2.SDL_RENDERER_PRESENTVSYNC)
    window.show()
def run_time(arg):
    global numbers
    numbers[arg]=time.time()-start
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
def run_sub(arg1,arg2,arg3):
    if str(arg1)[0]=="@":
        arg1=numbers[arg1]
    if str(arg2)[0]=="@":
        arg2=numbers[arg2]
    numbers[arg3] = arg1-arg2
def run_add(arg1,arg2,arg3):
    if str(arg1)[0]=="@":
        arg1=numbers[arg1]
    if str(arg2)[0]=="@":
        arg2=numbers[arg2]
    numbers[arg3] = arg1+arg2
def run_mul(arg1,arg2,arg3):
    if str(arg1)[0]=="@":
        arg1=numbers[arg1]
    if str(arg2)[0]=="@":
        arg2=numbers[arg2]
    numbers[arg3] = arg1*arg2
def run_div(arg1,arg2,arg3):
    if str(arg1)[0]=="@":
        arg1=numbers[arg1]
    if str(arg2)[0]=="@":
        arg2=numbers[arg2]
    numbers[arg3] = arg1/arg2
def run_mod(arg1,arg2,arg3):
    if str(arg1)[0]=="@":
        arg1=numbers[arg1]
    if str(arg2)[0]=="@":
        arg2=numbers[arg2]
    numbers[arg3] = arg1%arg2
def run_power(arg1,arg2,arg3):
    if str(arg1)[0]=="@":
        arg1=numbers[arg1]
    if str(arg2)[0]=="@":
        arg2=numbers[arg2]
    numbers[arg3] = arg1**arg2
def run_radical(arg1,arg2,arg3):
    if str(arg1)[0]=="@":
        arg1=numbers[arg1]
    if str(arg2)[0]=="@":
        arg2=numbers[arg2]
    numbers[arg3] = arg2 ** (1 / arg1)
def run_read(arg1,arg2):
    global strings
    f=open(arg1,"r")
    strings[arg2] = f.read()
    f.close()
def run_write(arg1,arg2):
    global strings
    f=open(arg1,"w")
    if (arg2.count("$")!=0):
        arg2 = strings[arg2]
    elif (arg2.count("@")!=0):
        arg2 = str(numbers[arg2])
    else:
        arg2 = arg2.replace('"',"")
    f.write(arg2)
    f.close()
def run_string(arg1,arg2=""):
    if (arg2.count("$")!=0):
        arg2 = strings[arg2]
    elif (arg2.count("@")!=0):
        arg2 = str(numbers[arg2])
    else:
        arg2 = arg2.replace('"',"")
    strings[arg1] = arg2
def run_replace(arg1,arg2,arg3,arg4):
    if (arg1.count("$")!=0):
        arg1 = strings[arg1]
    else:
        arg1 = arg1.replace('"',"")
    if (arg2.count("$")!=0):
        arg2 = strings[arg2]
    else:
        arg2 = arg2.replace('"',"")
    if (arg3.count("$")!=0):
        arg3 = strings[arg3]
    else:
        arg3 = arg3.replace('"',"")
    strings[arg4] = arg1.replace(arg2,arg3)
def run_split(arg1,arg2,arg3):
    if (arg1.count("$")!=0):
        arg1 = strings[arg1]
    else:
        arg1 = arg1.replace('"',"")
    if (arg2.count("$")!=0):
        arg2 = strings[arg2]
    else:
        arg2 = arg2.replace('"',"")
    strings[arg3]=arg1.split(arg2)
def run_index(arg1,arg2,arg3):
    if (arg1.count("$")!=0):
        arg1 = strings[arg1]
    else:
        arg1 = numbers[arg1]
    if (type(arg2)!=type(0)and type(arg2)!=type(0.1)):
        if (arg2.count("@")!=0):
            arg2 = numbers[arg2]
        elif (arg2.count("$")!=0):
            arg2 = parse_number(strings[arg2])

    if (arg3.startswith("$")):
        strings[arg3] = arg1[arg2]
    else:
        numbers[arg3] = arg1[arg2]
def run_len(arg1,arg2):
    if type(arg1)==type(0) or type(arg1)==type(0.1):
        arg1=str(arg1)
    if (arg1.count("$")!=0):
        arg1 = strings[arg1]
    elif (arg1.count("@")!=0):
        arg1 = numbers[arg1]
    numbers[arg2] = len(arg1)
def run_pc(arg1):
    numbers[arg1] = pc
def run_func(arg):
    global pc
    global functions
    if functions.get(arg)!={}.get("a"):
        pc=functions[arg]
    else:
        functions[arg]=pc
def run_delete(arg):
    if arg.count("@")!=0:
        numbers.__delitem__(arg)
    elif arg.count("$")!=0:
        strings.__delitem__(arg)
def run_int(arg1,arg2):
    if (arg1.count("@")!=0):
        arg1 = numbers[arg1]
    numbers[arg2] = int(arg1)
def run_float(arg1,arg2):
    if (arg1.count("@")!=0):
        arg1 = numbers[arg1]
    numbers[arg2] = float(arg1)
def run_max(arg1,arg2,arg3):
    if str(arg1)[0]=="@":
        arg1=numbers[arg1]
    if str(arg2)[0]=="@":
        arg2=numbers[arg2]
    numbers[arg3] = max(arg1,arg2)
def run_min(arg1,arg2,arg3):
    if str(arg1)[0]=="@":
        arg1=numbers[arg1]
    if str(arg2)[0]=="@":
        arg2=numbers[arg2]
    numbers[arg3] = min(arg1,arg2)
def run_sin(arg1,arg2):
    if str(arg1)[0]=="@":
        arg1=numbers[arg1]
    numbers[arg2] = math.sin(arg1)
def run_cos(arg1,arg2):
    if str(arg1)[0]=="@":
        arg1=numbers[arg1]
    numbers[arg2] = math.cos(arg1)
def run_debug():
    print("Numbers: ",str(numbers).replace(r"{"," ").replace(r"}"," ").replace(r":","="))
    print("Strings: ",str(strings).replace(r"{"," ").replace(r"}"," ").replace(r":","="))
    print("Addresses: ",str(addresses).replace(r"{"," ").replace(r"}"," ").replace(r":","="))
    print("Functions: ",str(functions).replace(r"{"," ").replace(r"}"," ").replace(r":","="))
def run_help():
    print("Redmond V0.3.0\n\n")
    for key in tooltip_descriptions:
        print(f"{key}: {tooltip_descriptions[key]} (Usage: {tooltip_arguments[key]})\n")
def run_jalloc(arg):
    with open(arg,"r") as f:
        alloc = json.load(f)
        for e in alloc:
            if e.startswith("@"):
                numbers[e]=alloc[e]
            elif e.startswith("$"):
                strings[e]=alloc[e]
def run_image(image, w, h, uv_x, uv_y, uv_w, uv_h, filter, out):
    if str(image)[0] == "$":
        image = strings[image]
    if str(w)[0] == "@":
        w = numbers[w]
    if str(h)[0] == "@":
        h = numbers[h]
    if str(uv_x)[0] == "@":
        uv_x = numbers[uv_x]
    if str(uv_y)[0] == "@":
        uv_y = numbers[uv_y]
    if str(uv_w)[0] == "@":
        uv_w = numbers[uv_w]
    if str(uv_h)[0] == "@":
        uv_h = numbers[uv_h]
    if str(filter)[0] == "@":
        filter = numbers[filter]
    if str(image)[0] == "@":
        image = numbers[image]
        width = image[0]
        height = image[1]
        pixels = image[2][0]
        surface = sdl2.SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, sdl2.SDL_PIXELFORMAT_RGBA32)
        sdl2.SDL_LockSurface(surface)
        pixel_ptr = ctypes.cast(surface.contents.pixels, ctypes.POINTER(ctypes.c_uint32))
        for y in range(height):
            for x in range(width):
                pixel_ptr[y * width + x] = pack_color(*pixels[x][y])
        sdl2.SDL_UnlockSurface(surface)
    else:
        surface = sdl2.SDL_LoadBMP(image.encode("utf-8"))
    if not surface:
        raise RuntimeError(f"Failed to load image: {image}")
    sw, sh = surface.contents.w, surface.contents.h
    crop_rect = sdl2.SDL_Rect(int(uv_x * sw), int(uv_y * sh), int(uv_w * sw), int(uv_h * sh))
    cropped = sdl2.SDL_CreateRGBSurfaceWithFormat(0, crop_rect.w, crop_rect.h, 32, sdl2.SDL_PIXELFORMAT_RGBA32)
    sdl2.SDL_BlitSurface(surface, crop_rect, cropped, None)
    if filter == 0:
        sdl2.SDL_SetHint(sdl2.SDL_HINT_RENDER_SCALE_QUALITY, b"0")
    elif filter == 1:
        sdl2.SDL_SetHint(sdl2.SDL_HINT_RENDER_SCALE_QUALITY, b"1")
    else:
        raise ValueError("Invalid filter mode. Use 0 for point, 1 for bilinear.")
    texture = sdl2.SDL_CreateTextureFromSurface(renderer, cropped)
    sdl2.SDL_SetTextureBlendMode(texture, sdl2.SDL_BLENDMODE_BLEND)
    images[out] = [texture, w, h, filter]
def run_blit(image, x, y, r):
    if str(r)[0] == "@":
        r = numbers[r]
    if str(x)[0] == "@":
        x = numbers[x]
    if str(y)[0] == "@":
        y = numbers[y]
    texture, w, h, filter = images[image]
    if filter == 0:
        sdl2.SDL_SetHint(sdl2.SDL_HINT_RENDER_SCALE_QUALITY, b"0")
    elif filter == 1:
        sdl2.SDL_SetHint(sdl2.SDL_HINT_RENDER_SCALE_QUALITY, b"1")
    dst_rect = sdl2.SDL_Rect(int(x - w // 2), int(y - h // 2), w, h)
    center = sdl2.SDL_Point(w // 2, h // 2)
    sdl2.SDL_RenderCopyEx(renderer, texture, None, dst_rect, float(r), center, sdl2.SDL_FLIP_NONE)
    if not useBuffer:
        sdl2.SDL_RenderPresent(renderer)
def run_dim(image, out):
    global numbers, strings
    if str(image)[0] == "$":
        image = strings[image]
    surface = sdl2.SDL_LoadBMP(image.encode('utf-8'))
    if not surface:
        raise RuntimeError(f"Failed to load image: {image}")
    w = surface.contents.w
    h = surface.contents.h
    numbers[out] = [w, h]
def run_cls():
    os.system('cls')
def run(stack):
    global numbers
    global addresses
    global pc
    is_string = False
    buffer=""
    for i in range(0,len(stack)):
        if stack[i]=='"':
            is_string = not is_string
        elif (stack[i]!=" " or is_string):
            buffer=buffer+stack[i]
        elif stack[i]==" ":
            buffer=buffer+","
    buffer=buffer.split(";")
    while buffer.count('')!=0:
        buffer.remove('')
    for i in range(len(buffer)):
        buffer[i]=buffer[i].split(",")
        for x in range(0,len(buffer[i])):
            if parse_number(buffer[i][x])!=None:
                buffer[i][x]=parse_number(buffer[i][x])
    stack=buffer
    #===========================================================================================================================================================
    functions = []
    for i in range(len(stack)):
        if stack[i][0]!="FUNC":
            continue
        if functions.__contains__(stack[i][1]):
            continue
        functions.append(stack[i][1])
    signatures = {}
    definition_indexes = {}
    for f in range(len(functions)):
        for index in range(len(stack)):
            i = len(stack)-1-index
            if not (stack[i][0]=="FUNC" and stack[i][1]==functions[f]):
                continue
            definition_indexes[functions[f]] = i
            signature = []
            for x in range(2,len(stack[i])):
                signature.append(stack[i][x])
            signatures[functions[f]] = signature
            stack[i] = ["FUNC",functions[f]]
            for y in range(i+1,len(stack)):
                if stack[y][0]=="RETURN":
                    stack[y] = ["ADD",3,"@"+functions[f]+"_return","@"+functions[f]+"_return"]
                    stack.insert(y+1,["JUMP","@"+functions[f]+"_return"])
            break
    buffer = []
    for i in range(len(stack)):
        buffer.append(stack[i])
    added=0
    for f in range(len(functions)):
        for i in range(len(stack)):
            if stack[i][0]!="FUNC" or stack[i][1]!=functions[f] or i==definition_indexes[functions[f]]:
                continue
            arguments = []
            for x in range(2,len(stack[i])):
                arguments.append(stack[i][x])
            for a in range(len(arguments)):
                if (signatures[functions[f]][a].startswith("@")):
                    buffer.insert(i+added,["NUM",signatures[functions[f]][a],arguments[a]])
                elif (signatures[functions[f]][a].startswith("$")):
                    buffer.insert(i+added,["STRING",signatures[functions[f]][a],arguments[a]])
                added=added+1
            buffer.insert(i+added,["NUM","@"+functions[f]+"_return"])
            added=added+1
            buffer.insert(i+added,["PC","@"+functions[f]+"_return"])
            added=added+1
            for a in range(len(arguments)):
                buffer.insert(i+added+1,["DELETE",signatures[functions[f]][a]])
                added=added+1
            buffer.insert(i+added+1,["DELETE","@"+functions[f]+"_return"])
            added=added+1
            buffer[i+added-(len(arguments)+1)] = ["FUNC",functions[f]]
    stack = buffer
    #===========================================================================================================================================================
    if len(stack[len(stack)-1][0])==0:
        stack.remove(stack[len(stack)-1])
    pc=0
    draw=False
    useBuffer=False
    for i in range(len(stack)):
        if stack[i][0]=="ADDRESS":
            pc=i
            run_address(stack[i][1])
    for i in range(len(stack)):
        if stack[(len(stack)-1)-i][0]=="FUNC":
            pc=(len(stack)-1)-i
            run_func(stack[(len(stack)-1)-i][1])
    pc=0
    event = sdl2.SDL_Event()
    running = True
    while pc!=-1 and pc<len(stack) and running:
        while sdl2.SDL_PollEvent(ctypes.byref(event)) != 0:
            if event.type == sdl2.SDL_QUIT:
                running = False
        if stack[pc][0]=="EXIT":
            run_exit()
        elif stack[pc][0]=="NUM":
            if len(stack[pc][0])>2:
                arg=0
                for i in range(2,len(stack[pc])):
                    if str(stack[pc][i])[0]=="@":
                        arg=arg+numbers[stack[pc][i]]
                    elif str(stack[pc][i])[0]=="$":
                        arg=arg+parse_number(strings[stack[pc][i]])
                    else:
                        arg=arg+parse_number(stack[pc][i])
                
                run_alloc(stack[pc][1],arg)
            else:
                run_alloc(stack[pc][1])
        elif stack[pc][0]=="PRINT":
            arg=""
            for i in range(1,len(stack[pc])):
                if str(stack[pc][i])[0]=="@":
                    arg=arg+str(numbers[stack[pc][i]])
                elif str(stack[pc][i])[0]=="$":
                    arg=arg+str(strings[stack[pc][i]])
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
        elif stack[pc][0]=="SUB":
            run_sub(stack[pc][1],stack[pc][2],stack[pc][3])
        elif stack[pc][0]=="ADD":
            run_add(stack[pc][1],stack[pc][2],stack[pc][3])
        elif stack[pc][0]=="MUL":
            run_mul(stack[pc][1],stack[pc][2],stack[pc][3])
        elif stack[pc][0]=="DIV":
            run_div(stack[pc][1],stack[pc][2],stack[pc][3])
        elif stack[pc][0]=="MODULUS":
            run_mod(stack[pc][1],stack[pc][2],stack[pc][3])
        elif stack[pc][0]=="POWER":
            run_power(stack[pc][1],stack[pc][2],stack[pc][3])
        elif stack[pc][0]=="RADICAL":
            run_radical(stack[pc][1],stack[pc][2],stack[pc][3])
        elif stack[pc][0]=="WRITE":
            run_write(stack[pc][1],stack[pc][2])
        elif stack[pc][0]=="READ":
            run_read(stack[pc][1],stack[pc][2])
        elif stack[pc][0]=="STRING":
            if len(stack[pc][0])>2:
                arg=""
                for i in range(2,len(stack[pc])):
                    if str(stack[pc][i])[0]=="@":
                        arg=arg+str(numbers[stack[pc][i]])
                    if str(stack[pc][i])[0]=="$":
                        arg=arg+str(strings[stack[pc][i]])
                    else:
                        arg=arg+str(stack[pc][i])
                run_string(stack[pc][1],arg)
            else:
                run_string(stack[pc][1])
        elif stack[pc][0]=="REPLACE":
            run_replace(stack[pc][1],stack[pc][2],stack[pc][3],stack[pc][4])
        elif stack[pc][0]=="INDEX":
            run_index(stack[pc][1],stack[pc][2],stack[pc][3])
        elif stack[pc][0]=="SPLIT":
            run_split(stack[pc][1],stack[pc][2],stack[pc][3])
        elif stack[pc][0]=="LEN":
            run_len(stack[pc][1],stack[pc][2])
        elif stack[pc][0]=="PC":
            run_pc(stack[pc][1])
        elif stack[pc][0]=="FUNC":
            run_func(stack[pc][1])
        elif stack[pc][0]=="DELETE":
            run_delete(stack[pc][1])
        elif stack[pc][0]=="INT":
            run_int(stack[pc][1],stack[pc][2])
        elif stack[pc][0]=="FLOAT":
            run_float(stack[pc][1],stack[pc][2])
        elif stack[pc][0]=="MIN":
            run_min(stack[pc][1],stack[pc][2],stack[pc][3])
        elif stack[pc][0]=="MAX":
            run_max(stack[pc][1],stack[pc][2],stack[pc][3])
        elif stack[pc][0]=="RECT":
            run_rect(stack[pc][1],stack[pc][2],stack[pc][3],stack[pc][4],stack[pc][5],stack[pc][6],stack[pc][7])
        elif stack[pc][0]=="GRADIENT":
            run_gradient(stack[pc][1],stack[pc][2],stack[pc][3],stack[pc][4],stack[pc][5],stack[pc][6],stack[pc][7],stack[pc][8],stack[pc][9],stack[pc][10],stack[pc][11])
        elif stack[pc][0]=="TRIG":
            run_trig(stack[pc][1],stack[pc][2],stack[pc][3],stack[pc][4],stack[pc][5],stack[pc][6],stack[pc][7],stack[pc][8],stack[pc][9])
        elif stack[pc][0]=="SIN":
            run_sin(stack[pc][1],stack[pc][2])
        elif stack[pc][0]=="COS":
            run_cos(stack[pc][1],stack[pc][2])
        elif stack[pc][0]=="DEBUG":
            run_debug()
        elif stack[pc][0]=="HELP":
            run_help()
        elif stack[pc][0]=="JALLOC":
            run_jalloc(stack[pc][1])
        elif stack[pc][0]=="IMAGE":
            run_image(stack[pc][1],stack[pc][2],stack[pc][3],stack[pc][4],stack[pc][5],stack[pc][6],stack[pc][7],stack[pc][8],stack[pc][9])
        elif stack[pc][0]=="BLIT":
            run_blit(stack[pc][1],stack[pc][2],stack[pc][3],stack[pc][4])
        elif stack[pc][0]=="CLS":
            run_cls()
        elif stack[pc][0]=="DIM":
            run_dim(stack[pc][1],stack[pc][2])
        pc=pc+1
import sys 
with open(".json/syntax.json","r") as f:
    syntax = json.load(f)
    comment = syntax["comment"]
    tooltip_descriptions = syntax["tooltip_descriptions"]
    tooltip_arguments = syntax["tooltip_arguments"]
with open(sys.argv[1]) as f:
    string = f.read()
    string = string.replace("\n\n","\n")
    string = string.replace("BREAKPOINT;","ADDRESS BREAKPOINT;\nADDRESS BREAKPOINT;")
    alloc = {}
    comment_start = re.escape(comment["start"])
    comment_end = re.escape(comment["end"])
    pattern = rf"{comment_start}.*?{comment_end}"
    string = re.sub(pattern, ";", string, re.DOTALL)
    stringJSON = re.findall(pattern, string, re.DOTALL)
    if (len(stringJSON)==0):
        run(string)
    else:
        stringJSON = re.findall(r"{.*?}", string, re.DOTALL)
        for i in range(len(stringJSON)):
            string = string.replace(stringJSON[i], "")
        string = string.replace("}", "")
        string = string.replace("{", "")
        string = string.replace("\n","")
        JSON = ""
        for i in range(len(stringJSON)):
            if stringJSON[i].count("}")!=stringJSON[i].count("{"):
                stringJSON[i]=stringJSON[i]+"\n}"
            if i!=len(stringJSON)-1:
                JSON=JSON+stringJSON[i]+",\n"
            else:
                JSON=JSON+stringJSON[i]+"\n"
        JSON = JSON.replace("\n},\n{",",")
        if (JSON!=""):
            alloc = json.loads(JSON)
            for e in alloc:
                if e.startswith("@"):
                    numbers[e]=alloc[e]
                elif e.startswith("$"):
                    strings[e]=alloc[e]
        run(string)
