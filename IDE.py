import keyboard
import tkinter as tk
from tkinter import filedialog
import os
import json
path = "unnamed.rdmnd"
undo = []
def open_file():
    global path
    get_path = filedialog.askopenfilename(
            title="Select a file",
            filetypes=[("Redmond", "*.rdmnd"), ("All files", "*.*")]
        )
    if (os.path.exists(get_path)and get_path!=''):
        path = get_path
        file = open(path,"r")
        text.delete("1.0",tk.END)
        text.insert(tk.END, file.read())
        file.close()
def save():
    file = open(path,"w")
    file.write(text.get("1.0", tk.END).strip())
    file.close()
def run():
        if (not os.path.exists("IDE.py")):
            os.system('start cmd /C"Redmond "'+path+'"')
        else:
            os.system('start cmd /C"python run.py "'+path+'"')
def undoCommand():
    if len(undo)>1:
        text.delete("1.0",tk.END)
        text.insert(tk.END, undo[-2])
        undo.pop()
        undo.pop()


def hotkey(e):
    if e.name == "s" and e.event_type == "down" and keyboard.is_pressed("ctrl"):
        save()
    if e.name == "r" and e.event_type == "down" and keyboard.is_pressed("ctrl"):
        run()
    if e.name == "z" and e.event_type == "down" and keyboard.is_pressed("ctrl"):
        undoCommand()
    if e.name == "o" and e.event_type == "down" and keyboard.is_pressed("ctrl"):
        open_file()

keyboard.hook(hotkey)
def loop_task():
    keyboard.clear_all_hotkeys()
    keyboard.add_hotkey("ctrl+s",save)
    keyboard.add_hotkey("ctrl+r",run)
    keyboard.add_hotkey("ctrl+z",undoCommand)
    new_width = int((80/1280)*window.winfo_width())
    new_height = int((30/800)*window.winfo_height())
    text.config(width=new_width, height=new_height)
    content = text.get("1.0", tk.END)
    if len(undo)==0:
        undo.append(content)
    elif not undo.__contains__(content):
        undo.append(content)
    list = content.split("\n")
    iterator = iter(highlights)
    for z in range(len(highlights)):
        key = next(iterator)
        for y in range(len(list)):
            for x in range(list[y].count(key)):
                index1 = str(y+1)+"."+str(list[y].find(key,x))
                index2 = str(y+1)+"."+str(list[y].find(key,x)+len(key))
                text.tag_add(highlights[key], index1, index2)
    window.after(100, loop_task)
window = tk.Tk()
window.title("Redmond IDE")
window.geometry("1280x720")
btn1 = tk.Button(window, text="Open", command=open_file)
btn1.place(relx=0,rely=0,anchor="nw")
btn2 = tk.Button(window, text="Save", command=save)
btn2.place(relx=1,rely=0,anchor="ne")
text = tk.Text(window, height=30, width=80, font=("Arial", 16))
text.place(relx=0.5, rely=0.5, anchor="center")
text.tag_configure("function", foreground="purple")
with open(".json/syntax.json", "r") as file:
    highlights = json.load(file)
if (os.path.exists(path)):
    file = open(path,"r")
    text.insert(tk.END, file.read())
    file.close()
loop_task()
window.mainloop()