# Flowchart — Enemy Detection, Pathfinding & Movement

```mermaid
flowchart TD
    A([START - Update / Frame]) --> B[Hitung distance antara Enemy dan Player]
    B --> C{distance <= detectionRadius ?}
    C -- Tidak --> Z1[Enemy Idle / Patrol]
    Z1 --> END([END frame])

    C -- Ya --> D{distance <= attackRange ?}
    D -- Ya --> Z2[Enemy Attack Player]
    Z2 --> END

    D -- Tidak --> E[Jalankan A-Star Pathfinding\nstart = posisi Enemy, goal = posisi Player]
    E --> F{Path ditemukan ?}
    F -- Tidak --> Z3[Enemy tetap diam / cari path lain]
    Z3 --> END

    F -- Ya --> G[Ambil waypoint berikutnya dari path]
    G --> H[Hitung direction = waypoint - posisi Enemy]
    H --> I[Gerakkan Enemy: posisi += direction.normalized * speed * deltaTime]
    I --> J{Sudah sampai di waypoint ?}
    J -- Ya --> K[Hapus waypoint dari list path]
    K --> END
    J -- Tidak --> END
```

### Penjelasan Tiap Blok
1. **Hitung distance** — jarak Euclidean `sqrt((px-ex)^2 + (py-ey)^2)`.
2. **Cek detectionRadius** — menentukan apakah player berada dalam jangkauan penglihatan/pendengaran enemy.
3. **Cek attackRange** — jika sudah sangat dekat, enemy tidak perlu pathfinding lagi, langsung menyerang.
4. **A\* Pathfinding** — mencari jalur terpendek dari posisi enemy ke posisi player melewati grid dungeon (menghindari tembok/obstacle).
5. **Movement (Seek)** — enemy bergerak waypoint demi waypoint mengikuti path yang dihasilkan A*, bukan bergerak lurus menembus tembok.
