# Enemy AI Dungeon — Tugas Game Development

**Studi Kasus:**
Player bergerak di dalam sebuah dungeon. Enemy harus:
1. Mendeteksi player,
2. Menentukan apakah player berada dalam jangkauan (range),
3. Mencari jalur (path) menuju player,
4. Bergerak menuju player mengikuti jalur tersebut.

---

# 1. Identifikasi Algoritma yang Digunakan

Studi kasus ini sebenarnya adalah **gabungan 3 algoritma** yang berjalan berurutan setiap frame:

| Tahap | Algoritma | Alasan Pemilihan |
|---|---|---|
| **1. Deteksi Player** | **Range/Distance Detection** (menghitung jarak Euclidean antara enemy & player, dibandingkan dengan `detectionRadius`) | Cara paling sederhana & murah secara komputasi (O(1) per enemy) untuk mengetahui apakah player "terlihat" oleh enemy. Sesuai materi *Time Complexity* di slide 15 — kita ingin operasi per-frame tetap ringan. |
| **2. Pencarian Jalur (Pathfinding)** | **A\* (A-Star) Search Algorithm** pada grid dungeon | Dungeon biasanya direpresentasikan sebagai grid/graph dengan tembok (obstacle). A* adalah algoritma pathfinding paling umum di industri game karena menggabungkan *Dijkstra* (cost dari start) dan *Greedy Best-First Search* (heuristic ke goal), sehingga jalur yang ditemukan **optimal** dan **efisien**, sesuai topik *Graph Algorithm* & *Pathfinding I/II* pada silabus mata kuliah ini. |
| **3. Pergerakan (Movement)** | **Seek / Steering Behaviour** (bagian dari Movement Algorithm — mengarahkan enemy ke *waypoint* berikutnya pada path hasil A*) | Setelah path ditemukan (berupa list of nodes/waypoints), enemy tidak langsung "teleport" ke player, melainkan bergerak halus node demi node menggunakan vektor arah (`direction = target - position`) dikali `speed`. Ini adalah bentuk dasar *Steering Behaviour* yang dibahas di topik *Movement Algorithm*. |

**Kesimpulan singkat:** Enemy menggunakan pola **Detect → Range Check → A\* Pathfinding → Seek Movement**, dieksekusi berulang setiap frame di dalam *game loop*.

Algoritma ini bersifat **deterministik** (bukan random) — untuk input posisi player & enemy yang sama, hasil path dan pergerakannya akan selalu sama (lihat slide 17–18 tentang *Deterministic Algorithm*).

---

# 2. Flowchart Algoritma

Diagram alur lengkap tersedia di [`docs/flowchart.md`](docs/flowchart.md) (format Mermaid, otomatis ter-render di GitHub).

Ringkasan alurnya:

```
START (setiap frame / update)
   |
   v
Hitung jarak (distance) Enemy <-> Player
   |
   v
Apakah distance <= detectionRadius? --(Tidak)--> Enemy diam / patrol -> (kembali ke START)
   |
  (Ya)
   |
   v
Apakah distance <= attackRange? --(Ya)--> Serang Player -> (kembali ke START)
   |
  (Tidak)
   |
   v
Jalankan A* Pathfinding (start=posisi enemy, goal=posisi player)
   |
   v
Apakah path ditemukan? --(Tidak)--> Enemy diam / idle -> (kembali ke START)
   |
  (Ya)
   |
   v
Ambil waypoint berikutnya dari path
   |
   v
Hitung arah (direction) menuju waypoint
   |
   v
Gerakkan enemy sepanjang direction * speed * deltaTime
   |
   v
Apakah sudah sampai di waypoint? --(Ya)--> Hapus waypoint dari list
   |
  (Tidak / lanjut)
   |
   v
END frame -> ulangi di frame berikutnya
```

---

# 3. Code Snippet

Bahasa: **C# (Unity Engine)** — karena paling umum dipakai untuk game 2D/3D dungeon crawler.

File lengkap: [`Scripts/EnemyAI.cs`](Scripts/EnemyAI.cs)

Kode mencakup:
- `DetectPlayer()` → Range/Distance Detection
- `FindPathAStar()` → implementasi A* sederhana di atas grid dungeon
- `MoveAlongPath()` → Seek Movement mengikuti waypoint hasil A*

---

# Struktur Repository

```
enemy-ai-dungeon/
├── README.md                <- penjelasan & jawaban tugas (file ini)
├── docs/
│   └── flowchart.md          <- flowchart (Mermaid)
└── Scripts/
    └── EnemyAI.cs             <- code snippet C# (Unity)
```
