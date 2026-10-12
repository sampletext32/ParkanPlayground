namespace ControlLib;

/// <summary>
/// Звено узла графа состояний (16 байт, <c>NavNodeLinkSlot</c> в Ghidra): точка опоры состояния. По флагу 0x1000 <c>BTLU_frame_update</c>
/// при касании опоры один раз выполняет собственный список команд звена (<c>extra</c>).
/// </summary>
/// <param name="Word0">+0x00: индекс контрольной точки CPT (<c>cpt_index</c>).</param>
/// <param name="Word1">+0x04: флаги звена (<c>flags</c>); 0x1000 - при касании опоры выполнить список команд <see cref="Word2"/>.</param>
/// <param name="Word2">+0x08: индекс списка команд, выполняемого при касании опоры (<c>extra</c>).</param>
/// <param name="Word3">+0x0C: не выяснено.</param>
public readonly record struct NavNodeLinkSlotRaw(uint Word0, uint Word1, uint Word2, uint Word3);
