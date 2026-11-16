# 文档与笔记索引

本目录存放项目相关的学习笔记、架构分析与设计记录。

> 放在仓库根目录而不是 `Assets/` 下，Unity 不会导入这些文件，也不会生成 `.meta` 或打进包体。

## 目录结构

```
Docs/
├─ README.md              ← 本索引
├─ NOTES-STYLE.md         ← 笔记规范（写之前先看）
├─ Templates/             ← 模板
├─ Notes/                 ← 学习笔记（按主题分类）
│  └─ Unity/
├─ Design/                ← 决策 / 计划 / TODO
└─ assets/                ← 笔记引用的截图
```

## 文档列表

| 主题 | 标题 | 日期 | 状态 | 链接 |
|---|---|---|---|---|
| Unity / 架构 | 输入系统升级：IInputSource + InputService | 2026-11-04 | 当前版本 | [input-system-upgrade.md](Notes/Unity/input-system-upgrade.md) |
| Unity / 资源 | 资源管理升级：ResourceScope + 并发加载去重 | 2026-11-05 | 已完成（未提交） | [resource-management-upgrade.md](Notes/Unity/resource-management-upgrade.md) |
| Unity / 架构 | 命名空间与文件归属约定 | 2026-11-16 | 当前版本 | [namespace-convention.md](Notes/Unity/namespace-convention.md) |
| 计划 | 资源管理升级计划（ResourceScope） | 2026-11-04 | 进行中 · `feature/resource-upgrade` | [2026-11-04-resource-upgrade-plan.md](Design/2026-11-04-resource-upgrade-plan.md) |
| TODO | 资源管理问题修复 TODO | 2026-11-10 | 待执行 | [2026-11-10-resource-fix-todo.md](Design/2026-11-10-resource-fix-todo.md) |
| 计划 | UI 层 MVVM 改造计划 | 2026-11-16 | 草稿 · `refactor/mvvm-ui` | [2026-11-16-mvvm-refactor-plan.md](Design/2026-11-16-mvvm-refactor-plan.md) |

## 笔记规范

完整规则见 [NOTES-STYLE.md](NOTES-STYLE.md)。核心三条：

1. **结论先行**：开头 3 行说清结论；
2. **短**：一篇 ≤ 120 行，代码片段 ≤ 15 行；
3. **可执行**：待办用 `- [ ]`，带验收标准。

模板：[Templates/note-template.md](Templates/note-template.md)。

## 新增笔记步骤

1. 复制 `Templates/note-template.md` 到对应目录；
2. 写完后按规范自检；
3. 在上面的「文档列表」加一行；
4. 提交时一起提交 `Docs/`。
