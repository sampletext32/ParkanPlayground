Допустим, игровой объект — **танк/бот с башней, пушкой и ракетницей**.

```text
CAniMesh of TankObject
│
├─ load_id = 0: "tank_body.msh"
│  source MSH 0x01 nodes:
│
│      node 0  -> piece[0]  "body_root"        parent = -1
│      node 1  -> piece[1]  "left_track"       parent = piece[0]
│      node 2  -> piece[2]  "right_track"      parent = piece[0]
│      node 3  -> piece[3]  "turret_socket"    parent = piece[0]
│      node 4  -> piece[4]  "hatch"            parent = piece[3]
│
│
├─ load_id = 1: "turret.msh"
│  attach_parent_absolute_piece_index = 3
│
│      node 0  -> skipped virtual attach root
│      node 1  -> piece[5]  "turret_base"      parent = piece[3]
│      node 2  -> piece[6]  "turret_rotor"     parent = piece[5]
│      node 3  -> piece[7]  "gun_socket"       parent = piece[6]
│      node 4  -> piece[8]  "rocket_socket"    parent = piece[6]
│
│
├─ load_id = 2: "cannon.msh"
│  attach_parent_absolute_piece_index = 7
│
│      node 0  -> skipped virtual attach root
│      node 1  -> piece[9]   "cannon_body"     parent = piece[7]
│      node 2  -> piece[10]  "cannon_barrel"   parent = piece[9]
│      node 3  -> piece[11]  "muzzle"          parent = piece[10]
│
│
└─ load_id = 3: "rocketlauncher.msh"
   attach_parent_absolute_piece_index = 8

       node 0  -> skipped virtual attach root
       node 1  -> piece[12]  "launcher_body"   parent = piece[8]
       node 2  -> piece[13]  "left_rocket"     parent = piece[12]
       node 3  -> piece[14]  "right_rocket"    parent = piece[12]
```

Итоговая иерархия `pieces_vector` выглядит уже как одно дерево:

```text
piece[0] body_root                     load_id = 0
├─ piece[1] left_track                 load_id = 0
├─ piece[2] right_track                load_id = 0
└─ piece[3] turret_socket              load_id = 0
   ├─ piece[4] hatch                   load_id = 0
   └─ piece[5] turret_base             load_id = 1
      └─ piece[6] turret_rotor         load_id = 1
         ├─ piece[7] gun_socket        load_id = 1
         │  └─ piece[9] cannon_body    load_id = 2
         │     └─ piece[10] cannon_barrel
         │        └─ piece[11] muzzle
         │
         └─ piece[8] rocket_socket     load_id = 1
            └─ piece[12] launcher_body load_id = 3
               ├─ piece[13] left_rocket
               └─ piece[14] right_rocket
```

Ключевой момент:

```text
load_id группирует pieces по исходному .msh,
а parent_piece_index строит единую иерархию внутри CAniMesh.
```

То есть после всех загрузок движок уже не обязан думать “это отдельная модель башни, это отдельная модель пушки”. Для поз, рендера и обхода геометрии это просто один `CAniMesh` с одним плоским массивом pieces и parent-связями между ними.

## Вывод

По сути получается, что .msh это набор деталей.
CAniMesh всегда имеет 1 root модель и может иметь "приклееные" детали. 
При этом он самостоятельно выполняет перепривязку "приклееных" деталей. 
Например, если "приклеиваемая" модель имеет 2 детали с каким-то parent, то
CAniMesh сдвинет их parent так, чтобы указывать в нужное место.



















