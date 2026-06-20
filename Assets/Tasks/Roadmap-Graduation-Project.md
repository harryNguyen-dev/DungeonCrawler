# DungeonCrawler — Roadmap cho đồ án tốt nghiệp

> Góc nhìn Tech Lead + PM, **không** hướng store/commercial. Mục tiêu: **bảo vệ được + demo ổn + báo cáo có chiều sâu kỹ thuật**.

## Mục tiêu khác với sản phẩm thương mại

| Thương mại | Đồ án tốt nghiệp |
|------------|------------------|
| Retention, monetization, store | **Điểm mới kỹ thuật**, methodology, đánh giá |
| Scale, live ops | **Demo end-to-end 10–15 phút** không crash |
| Analytics, ads, cloud | **Tài liệu + sơ đồ + test case** có thể trình bày |

**Exit criteria đồ án:** Hội đồng chơi được 1 run hoàn chỉnh, hiểu bạn làm gì và tại sao, có số liệu/bảng so sánh nhỏ (không cần user thật).

---

## P0 — Bắt buộc để nộp & bảo vệ

**Demo gameplay (đủ 1 lần chơi ấn tượng)**
- [ ] Flow: Lobby → chọn map → dungeon sinh procedural → combat → card → boss → win/lose → về hub
- [ ] Ít nhất **1 chapter hoàn chỉnh** (3–5 map unlock tuần tự là đủ; 10 map là bonus)
- [ ] Meta cơ bản: sao, unlock map, meta gold, chọn hero — **1 luồng rõ**, không cần shop đầy đủ
- [ ] Không crash trong demo 15 phút; restart/về lobby không bug singleton

**Điểm nhấn kỹ thuật (phải nói được trong báo cáo)**
- [ ] **WFC / procedural dungeon** — giải thích input, constraint, output; có ảnh/chụp màn hình nhiều seed
- [ ] **Enemy AI** — ít nhất 2 archetype (melee + ranged hoặc boss pattern) + sơ đồ state/flow
- [ ] **In-run build (card/skill)** — data-driven qua ScriptableObject; 1 diagram kiến trúc
- [ ] **Kiến trúc code** — event (`GlobalEvents`), manager, pooling, save local — 1 sơ đồ tổng quan

**Tài liệu đồ án (quan trọng ngang code)**
- [ ] Chương phân tích: bài toán, mục tiêu, phạm vi, use case
- [ ] Chương thiết kế: kiến trúc, class diagram, sequence (load scene, generate dungeon, end run)
- [ ] Chương cài đặt + **demo screenshot/GIF**
- [ ] Chương **đánh giá**: test case bảng, thời gian generate, FPS, so sánh 2–3 seed hoặc 2 cấu hình wave
- [ ] Kết luận: hạn chế + hướng phát triển (không cần làm hết)

**Build nộp**
- [ ] 1 bản build Windows (hoặc Android nếu GV yêu cầu) + hướng dẫn chạy 1 trang
- [ ] Video demo 3–5 phút (backup nếu máy hội đồng lỗi)

---

## P1 — Nên có để điểm cao

**Trình bày & UX demo**
- [ ] Loading/boot mượt; màn win/lose hiển thị sao + thống kê run
- [ ] Pause cơ bản hoặc ít nhất **không** có nút “coming soon” trong luồng demo
- [ ] Settings tối thiểu: âm lượng (chứng minh polish, không bắt buộc nhiều tùy chọn)
- [ ] 1 màn **tutorial ngắn** hoặc tooltip lần đầu (slide trong báo cáo = “thiết kế UX”)

**Chiều sâu học thuật**
- [ ] **So sánh/thử nghiệm WFC**: tham số grid, số room, tỷ lệ fail/retry — bảng 5–10 lần chạy
- [ ] **Balance note**: win rate map demo, thời gian run trung bình (playtest 5–10 người lớp là đủ)
- [ ] Liên hệ lý thuyết: roguelite loop, procedural gen, data-oriented design (trích dẫn 2–3 nguồn)

**Chất lượng code (GV hay hỏi)**
- [ ] README repo: cách mở project, scene chính, cấu trúc folder
- [ ] Vài test Play Mode cho phần **pure logic** (star calculator, save, WFC helper nếu tách được)
- [ ] Không dead code / placeholder log trong path validation demo

---

## P2 — Có thì tốt, **cắt được** nếu thiếu thời gian

- Store listing, IAP, ads, analytics
- Cloud save, đa nền tảng, localization
- Chapter 2+, daily quest, battle pass
- Shop cosmetic (Frames/Icons)
- CI/CD production-grade

→ Ghi vào **“Hướng phát triển”** chương kết luận là đủ.

---

## Phạm vi đề xuất (scope an toàn)

```
Must demo:  WFC dungeon + combat + card pick + 1 boss + meta unlock
Should doc: Kiến trúc + AI + save + pooling + đánh giá hiệu năng
Can skip:   Monetization, live ops, multi-platform
```

**Thời gian gợi ý (8–12 tuần)**

| Tuần | Việc |
|------|------|
| 1–2 | Chốt phạm vi đồ án + outline báo cáo |
| 3–5 | P0 gameplay ổn + diagram kiến trúc |
| 6–7 | Chương WFC/AI + thí nghiệm nhỏ |
| 8 | Polish demo + video + build |
| 9–10 | Viết báo cáo, slide bảo vệ |
| 11–12 | Dry-run bảo vệ, fix bug demo |

---

## Slide bảo vệ — 5 ý bắt buộc

1. **Bài toán & mục tiêu** — roguelite dungeon crawler procedural
2. **Kiến trúc hệ thống** — scene flow, events, data SO
3. **WFC** — thuật toán, ví dụ seed, hạn chế
4. **Gameplay loop** — combat, card, progression
5. **Đánh giá** — test case, FPS, thời gian generate, nhận xét

---

## So với file `Roadmap-Release-P0-P1-P2.md`

- **Bỏ / hạ:** monetization, store, retention KPI, soft launch
- **Nâng:** báo cáo, sơ đồ, thí nghiệm WFC, video demo, README
- **P0 thương mại** ≈ **P0 đồ án gameplay**; phần store/analytics chuyển sang P2 hoặc bỏ hẳn
