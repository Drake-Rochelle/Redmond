import sys
import keyboard
import tkinter as tk
from tkinter import filedialog
import os
import json
import ctypes
def is_admin():
    try:
        return ctypes.windll.shell32.IsUserAnAdmin()
    except:
        return False
if not (is_admin()or(os.path.exists("Redmond IDE.py"))):
    ctypes.windll.shell32.ShellExecuteW(
        None, "runas", sys.executable, " ".join(sys.argv), None, 1
    )
    sys.exit()
path = ""
undo = []
redo = []
if (os.path.exists("Redmond IDE.py")):
    keep_console = True
else:
    keep_console = False
def open_file():
    global path
    if getattr(sys, 'frozen', False):
        base_path = os.path.dirname(os.path.dirname(sys.executable))
    else:
        base_path = os.path.dirname(os.path.abspath(__file__))
    get_path = filedialog.askopenfilename(
            title="Select a file",
            filetypes=[("Redmond", "*.rdmnd"), ("All files", "*.*")],
            initialdir=base_path
        )
    if (os.path.exists(get_path)and get_path!=''):
        path = get_path
        file = open(path,"r")
        text.delete("1.0",tk.END)
        text.insert(tk.END, file.read())
        file.close()
def save():
    global path
    if (path == ""):
        if getattr(sys, 'frozen', False):
            base_path = os.path.dirname(os.path.dirname(sys.executable))
        else:
            base_path = os.path.dirname(os.path.abspath(__file__))
        get_path = filedialog.asksaveasfilename(
            title="Create New File",
            defaultextension=".rdmnd",
            filetypes=[("Redmond Scripts", "*.rdmnd"), ("All Files", "*.*")],
            initialdir=base_path
        )
        if ((not os.path.exists(get_path)) and get_path!=''):
            path = get_path
    file = open(path,"w")
    file.write(text.get("1.0", tk.END).strip())
    file.close()
def run():
        console = keep_console*"/K " + (not keep_console)*"/C "
        if (not os.path.exists("Redmond IDE.py")):
            run = False
            if "REDMOND_CORE" in text.get("1.0", tk.END):
                run = True
                os.system('start cmd '+console+'"Redmond_Core "'+path+'"')
            if (not run):
                os.system('start cmd '+console+'"Redmond_Core "'+path+'"')
        else:
            run = False
            if "REDMOND_CORE" in text.get("1.0", tk.END):
                run = True
                os.system('start cmd '+console+'"python Redmond_Core.py "'+path+'"')
            if "REDMOND_PYGAME" in text.get("1.0", tk.END):
                run = True
                os.system('start cmd '+console+'"python Redmond_Pygame.py "'+path+'"')
            if (not run):
                os.system('start cmd '+console+'"python Redmond_Core.py "'+path+'"')
def undoCommand():
    if len(undo)>1:
        text.delete("1.0",tk.END)

        
        text.insert(tk.END, undo[-2])
        redo.append(undo.pop())
        undo.pop()
def redoCommand():
    if len(redo)>0:
        text.delete("1.0",tk.END)
        text.insert(tk.END, redo[-1])
        undo.append(redo.pop())
def new_file():
    global path
    if getattr(sys, 'frozen', False):
        base_path = os.path.dirname(os.path.dirname(sys.executable))
    else:
        base_path = os.path.dirname(os.path.abspath(__file__))
    get_path = filedialog.asksaveasfilename(
        title="Create New File",
        defaultextension=".rdmnd",
        filetypes=[("Redmond Scripts", "*.rdmnd"), ("All Files", "*.*")],
        initialdir=base_path
    )
    if ((not os.path.exists(get_path)) and get_path!=''):
        path = get_path
        file = open(path,"w")
        file.write("")
        file.close()
        text.delete("1.0",tk.END)
def toggle_keep_console():
    global keep_console
    keep_console = not keep_console
def hotkey(e):
    if e.name == "s" and e.event_type == "down" and keyboard.is_pressed("ctrl"):
        save()
    if e.name == "r" and e.event_type == "down" and keyboard.is_pressed("ctrl"):
        run()
    if e.name == "z" and e.event_type == "down" and keyboard.is_pressed("ctrl"):
        undoCommand()
    if e.name == "o" and e.event_type == "down" and keyboard.is_pressed("ctrl"):
        open_file()
    if e.name == "y" and e.event_type == "down" and keyboard.is_pressed("ctrl"):
        redoCommand()
    if e.name == "n" and e.event_type == "down" and keyboard.is_pressed("ctrl"):
        new_file()
    if e.name == "t" and e.event_type == "down" and keyboard.is_pressed("ctrl"):
        toggle_tooltips()
    if e.name == "k" and e.event_type == "down" and keyboard.is_pressed("ctrl"):
        toggle_keep_console()
showingTooltip = False
tooltips = True
popup = None
def get_line_at_index(index):
    line_start = index.split(".")[0] + ".0"
    line_end = index.split(".")[0] + ".end"
    line = text.get(line_start, line_end)
    line = line.split(" ")[0]
    line = line.split(";")[0]
    return line
def HideTooltip():
    global showingTooltip
    global popup
    showingTooltip = False
    popup.destroy()
    popup = None
def toggle_tooltips():
    global tooltips
    tooltips = not tooltips
    if (showingTooltip):
        HideTooltip()
def ShowTooltip(x,y):
    global showingTooltip
    global popup
    if (popup!=None):
        popup.destroy()
        popup = None
    showingTooltip = True
    popup = tk.Toplevel()
    popup.wm_overrideredirect(True)
    x_root = text.winfo_rootx() + x+15
    y_root = text.winfo_rooty() + y-15
    popup.geometry(f"+{x_root}+{y_root}")
    word = get_line_at_index(text.index(f"@{x},{y}"))
    if ((not tooltip_arguments.__contains__(word)) or (not tooltip_descriptions.__contains__(word))):
        label = tk.Label(popup, text="This function does not yet have a description ( °-° )", background="white", relief="solid", borderwidth=1,font=(tooltip["font"], tooltip["size"]))
    else:
        label = tk.Label(popup, text=tooltip_descriptions[word]+"\nUsage: "+tooltip_arguments[word], background="white", relief="solid", borderwidth=1,font=(tooltip["font"], tooltip["size"]))
    label.pack()
def on_motion(event):
    if (not tooltips):
        return
    index = text.index(f"@{event.x},{event.y}")
    tags = text.tag_names(index)
    if (len(tags)==0 or (not tags.__contains__("function"))):
        if (showingTooltip):
            HideTooltip()
        return
    ShowTooltip(event.x,event.y)
keyboard.hook(hotkey)
def loop_task():
    pos = 0
    text.config(font = (code["font"],int((16/800)*window.winfo_height())))
    content = text.get("1.0", tk.END)
    if len(undo)==0:
        undo.append(content)
    elif not undo.__contains__(content)and not keyboard.is_pressed("ctrl"):
        undo.append(content)
    text.tag_remove("arguments","1.0",tk.END)
    text.tag_remove("comment","1.0",tk.END)
    text.tag_remove("function","1.0",tk.END)
    list = content.split("\n")
    if (comment["start"]!=comment["end"]):
        for y in range(len(list)):
            for x in range(list[y].count(comment["start"])):
                index1 = str(y+1)+"."+str((list[y]+"\n").find(comment["start"],x)+1)
                index2 = str(y+1)+"."+str((list[y]+"\n").find(comment["end"],x)+len(comment["end"]))
                text.tag_add("comment", index1, index2)
    else:
        for y in range(len(list)):
            for x in range(0,list[y].count(comment["start"],2)):
                index1 = str(y+1)+"."+str((list[y]+"\n").find(comment["start"],x))
                index2 = str(y+1)+"."+str((list[y]).find(comment["start"],x+1)+len(comment["start"]))
                text.tag_add("comment", index1, index2)
    if (arguments["start"]!=arguments["end"]):
        for y in range(len(list)):
            for x in range(list[y].count(arguments["start"])):
                index1 = str(y+1)+"."+str((list[y]+"\n").find(arguments["start"],x)+1)
                index2 = str(y+1)+"."+str((list[y]+"\n").find(arguments["end"],x)+len(arguments["end"])-1)
                text.tag_add("arguments", index1, index2)
    else:
        for y in range(len(list)):
            for x in range(0,list[y].count(arguments["start"],2)):
                index1 = str(y+1)+"."+str((list[y]+"\n").find(arguments["start"],x))
                index2 = str(y+1)+"."+str((list[y]).find(arguments["start"],x+1)+len(arguments["start"]))
                text.tag_add("arguments", index1, index2)
    iterator = iter(highlights)
    for z in range(len(highlights)):
        key = next(iterator)
        for y in range(len(list)):
            for x in range(list[y].count(key)):
                index1 = str(y+1)+"."+str(list[y].find(key,x))
                index2 = str(y+1)+"."+str(list[y].find(key,x)+len(key))
                text.tag_add(highlights[key], index1, index2)
    text.tag_raise("function","arguments")
    window.after(100, loop_task)
with open(".json/syntax.json", "r") as file:
    syntax = json.load(file)
    highlights = syntax["words"]
    comment = syntax["comment"]
    arguments = syntax["arguments"]
    tooltip_descriptions = syntax["tooltip_descriptions"]
    tooltip_arguments = syntax["tooltip_arguments"]
    code = syntax["code"]
    tooltip = syntax["tooltip"]
window = tk.Tk()
window.title("Redmond IDE")
window.geometry("1280x720")

menu_bar = tk.Menu(window)

file_menu = tk.Menu(menu_bar, tearoff=0)
file_menu.add_command(label="New (Ctrl + N)", command=new_file)
file_menu.add_command(label="Open (Ctrl + O)", command=open_file)
file_menu.add_command(label="Save (Ctrl + S)", command=save)
menu_bar.add_cascade(label="File", menu=file_menu)

preference_menu = tk.Menu(menu_bar, tearoff=0)
preference_menu.add_command(label="Toggle Tooltips (Ctrl+T)", command=toggle_tooltips)
preference_menu.add_command(label="Toggle Keep Console (Ctrl+K)", command=toggle_keep_console)
menu_bar.add_cascade(label="Preferences", menu=preference_menu)

run_menu = tk.Menu(menu_bar, tearoff=0)
run_menu.add_command(label="Run (Ctrl+R)", command=run)
menu_bar.add_cascade(label="Run", menu=run_menu)

window.config(menu=menu_bar)
text = tk.Text(window, height=30, width=80, font=(code["font"], 16))
text.place(relx=0.5, rely=0.5, anchor="center")
text.tag_configure("function", foreground="purple")
text.tag_configure("comment", foreground="green")
text.tag_configure("arguments", foreground="blue")
text.bind("<Motion>", on_motion)
if (os.path.exists(path)):
    file = open(path,"r")
    text.insert(tk.END, file.read())
    file.close()
loop_task()
window.mainloop()