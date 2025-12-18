# ABOUT

Redmond is an interpreter and IDE for a new programming language designed with efficiency and portability as the main priorities. There is currently an interpreter for Unity Engine and for Python. This language runs at 70% the speed of the base language (so, a given script will run at 70% the speed of C# using the C# interpreter and 70% the speed of Python using the Python interpreter. Included in the releases and the source is the documentation.

<img width="1282" height="752" alt="Screenshot 2025-09-29 093547" src="https://github.com/user-attachments/assets/51baa027-e579-4f18-90ef-0c4ec515aa88" />
<img width="1282" height="772" alt="Screenshot 2025-09-29 104116" src="https://github.com/user-attachments/assets/2b6abb8f-223d-4a94-ad07-7ffcf71493ba" />
<img width="1282" height="752" alt="Screenshot 2025-09-29 095838" src="https://github.com/user-attachments/assets/f4ef3d14-58ad-4d5f-8503-0b766cd9c2ef" />

# Example standalone script:
```
CONTEXT "local://contexts\buffer.rdmnd";
REDMOND_SCENE;
{"$SHADOW":"example_data/image/shadow.bmp","@HEIGHT":200,"$IMAGE":"example_data/image/Forza_horizon_3_cover_art.bmp","@R":0,"@XP":0,"@YP":0,"@MOVEMENT_SPEED":500,"@RUN_ADD":300,"@PR":0,"@DT":0,"@WINDOW":[2560,1600]}
@START = TIME;
NUM @A,90;
@WINDOW_WIDTH = INDEX(@WINDOW,0);
@WINDOW_HEIGHT = INDEX(@WINDOW,1);
WINDOW 1,@WINDOW_WIDTH,@WINDOW_HEIGHT;
@WINDOW_WIDTH2 = DIV(@WINDOW_WIDTH,2);
@WINDOW_HEIGHT2 = DIV(@WINDOW_HEIGHT,2);
@SCALE = DIV(@WINDOW_HEIGHT,1600);
@DIM = DIM($IMAGE);
@W = INDEX(@DIM,0);
@H = INDEX(@DIM,1);
@W = MUL(2,@W);
@H = MUL(2,@H);
@GW = MUL(30,@W);
@GH = MUL(30,@H);
@SW = MUL(1.2,@W);
@SH = MUL(1.2,@H);
@W = MUL(@W,@SCALE);
@H = MUL(@H,@SCALE);
@GW = MUL(@GW,@SCALE);
@GH = MUL(@GH,@SCALE);
@SW = MUL(@SW,@SCALE);
@SH = MUL(@SH,@SCALE);
@PW = MUL(60,@SCALE);
NUM(@PH,@PW);
@PSW = MUL(72,@SCALE);
NUM(@PSH,@PSW);
@W = INT(@W);
@H = INT(@H);
@GW = INT(@GW);
@GH = INT(@GH);
@SW = INT(@SW);
@SH = INT(@SH);
@PW = INT(@PW);
@PH = INT(@PH);
@PSW = INT(@PSW);
@PSH = INT(@PSH);
#IMAGE = IMAGE($IMAGE,@W,@H,0,0,1,1,1);
#PLAYER = IMAGE($IMAGE,@PW,@PH,0,0,1,1,1);
#GROUND = IMAGE($IMAGE,@GW,@GH,0,0,1,1,1);
#SHADOW = IMAGE($SHADOW,@SW,@SH,0,0,1,1,1);
#PLAYER_SHADOW = IMAGE($SHADOW,@PSW,@PSH,0,0,1,1,1);
@HEIGHT = MUL(@HEIGHT,@SCALE);
@HEIGHT2 = DIV(@HEIGHT,2);
@HEIGHT4 = DIV(@HEIGHT2,8);
@HEIGHT8 = DIV(@HEIGHT4,2);
LOOP = ADDRESS;
@RUN = KEY(shift);
@RUN = MUL(@RUN,@RUN_ADD);
@RUN = MUL(@RUN,@SCALE);
RECT(0,0,@WINDOW_WIDTH,@WINDOW_HEIGHT,0,0,0);
NUM(@X);
NUM(@Y);
@X = ADD(@WINDOW_WIDTH2,@X);
@Y = ADD(@WINDOW_HEIGHT2,@Y);
@X = SUB(@X,@XP);
@Y = SUB(@Y,@YP);
BLIT(#GROUND,@X,@Y,0);
@PSX = ADD(@WINDOW_WIDTH2,@HEIGHT8);
@PSY = ADD(@WINDOW_HEIGHT2,@HEIGHT4);
BLIT(#PLAYER_SHADOW,@PSX,@PSY,0);
BLIT(#PLAYER,@WINDOW_WIDTH2,@WINDOW_HEIGHT2,@PR);
@Y = ADD(@HEIGHT,@Y);
@X = ADD(@HEIGHT2,@X);
BLIT(#SHADOW,@X,@Y,@R);
@Y = SUB(@Y,@HEIGHT);
@X = SUB(@X,@HEIGHT2);
BLIT(#IMAGE,@X,@Y,@R);
BUFFER;
@END = TIME;
@DT = SUB(@END,@START);
@DT = ADD(@DT,0.0000001);
@FPS = DIV(1,@DT);
SHOWME(@FPS);
@START = TIME;
@END = MUL(100,@DT);
@R = ADD(@END,@R);
FUNC(HANDLE_MOVEMENT);
@QUIT = KEY(esc);
GREATER(@QUIT,0,EXIT);
ADDRESS(LOOP);
HANDLE_MOVEMENT = FUNC;
@MOVE = MUL(@MOVEMENT_SPEED,@DT);
@MOVE_RUN = MUL(@RUN,@DT);
@MOVE = ADD(@MOVE,@MOVE_RUN);
@MOVE = MUL(@MOVE,@SCALE);
@W_KEY = KEY(w);
@A_KEY = KEY(a);
@S_KEY = KEY(s);
@D_KEY = KEY(d);
GREATER(@W_KEY,0,W_KEY);
W_RETURN = ADDRESS;
GREATER(@A_KEY,0,A_KEY);
A_RETURN = ADDRESS;
GREATER(@S_KEY,0,S_KEY);
S_RETURN = ADDRESS;
GREATER(@D_KEY,0,D_KEY);
D_RETURN = ADDRESS;
GREATER(1,0,RETURN);
W_KEY = ADDRESS;
NUM(@PR,0);
@YP = SUB(@YP,@MOVE);
GREATER(1,0,W_RETURN);
A_KEY = ADDRESS;
NUM(@PR,-90);
@XP = SUB(@XP,@MOVE);
GREATER(1,0,A_RETURN);
S_KEY = ADDRESS;
NUM(@PR,180);
@YP = ADD(@YP,@MOVE);
GREATER(1,0,S_RETURN);
D_KEY = ADDRESS;
NUM(@PR,90);
@XP = ADD(@XP,@MOVE);
GREATER(1,0,D_RETURN);
RETURN = ADDRESS;
RETURN;
EXIT = ADDRESS;
EXIT;
```

# Example Unity Script:
```
REDMOND_SCENE;
CAM 0;
C#_START
GameObject go = new GameObject();
MeshFilter mf = go.AddComponent<MeshFilter>();
MeshRenderer mr = go.AddComponent<MeshRenderer>();
Mesh mesh = new Mesh();
List<Vector3> verts = new List<Vector3>();
List<int> trigs = new List<int>();
verts.Add(new Vector3(0,0,2));
verts.Add(new Vector3(1,0,2));
verts.Add(new Vector3(0,1,2));
verts.Add(new Vector3(1,1,3));
trigs.Add(1);
trigs.Add(0);
trigs.Add(2);
trigs.Add(1);
trigs.Add(2);
trigs.Add(3);
mesh.vertices = verts.ToArray();
mesh.triangles = trigs.ToArray();
mf.mesh = mesh;
mr.material = meshFilter.GetComponent<MeshRenderer>().material;
float x = 0;
float y = 0;
float z = 0;
C#_END
ADDRESS F;
@d = KEY("d");
GREATER 1,@d,a;
C#_START
x = x + 0.01f;
C#_END
ADDRESS a;
@a = KEY("a");
GREATER 1,@a,w;
C#_START
x = x - 0.01f;
C#_END
ADDRESS w;
@w = KEY("w");
GREATER 1,@w,s;
C#_START
z = z + 0.01f;
C#_END
ADDRESS s;
@s = KEY("s");
GREATER 1,@s,z;
C#_START
z = z - 0.01f;
C#_END
ADDRESS z;
@z = KEY("z");
GREATER 1,@z,x;
C#_START
y = y - 0.01f;
C#_END
ADDRESS x;
@x = KEY("x");
GREATER 1,@x,R;
C#_START
y = y + 0.01f;
C#_END
ADDRESS R;
C#_START
go.transform.position = new Vector3(x,y,z);
C#_END
BUFFER;
ADDRESS F;
```
