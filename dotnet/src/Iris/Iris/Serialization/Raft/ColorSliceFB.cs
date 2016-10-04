// automatically generated by the FlatBuffers compiler, do not modify

namespace Iris.Serialization.Raft
{

using System;
using FlatBuffers;

public struct ColorSliceFB : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static ColorSliceFB GetRootAsColorSliceFB(ByteBuffer _bb) { return GetRootAsColorSliceFB(_bb, new ColorSliceFB()); }
  public static ColorSliceFB GetRootAsColorSliceFB(ByteBuffer _bb, ColorSliceFB obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
  public ColorSliceFB __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public uint Index { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetUint(o + __p.bb_pos) : (uint)0; } }
  public ColorSpaceFB? Value { get { int o = __p.__offset(6); return o != 0 ? (ColorSpaceFB?)(new ColorSpaceFB()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }

  public static Offset<ColorSliceFB> CreateColorSliceFB(FlatBufferBuilder builder,
      uint Index = 0,
      Offset<ColorSpaceFB> ValueOffset = default(Offset<ColorSpaceFB>)) {
    builder.StartObject(2);
    ColorSliceFB.AddValue(builder, ValueOffset);
    ColorSliceFB.AddIndex(builder, Index);
    return ColorSliceFB.EndColorSliceFB(builder);
  }

  public static void StartColorSliceFB(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddIndex(FlatBufferBuilder builder, uint Index) { builder.AddUint(0, Index, 0); }
  public static void AddValue(FlatBufferBuilder builder, Offset<ColorSpaceFB> ValueOffset) { builder.AddOffset(1, ValueOffset.Value, 0); }
  public static Offset<ColorSliceFB> EndColorSliceFB(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<ColorSliceFB>(o);
  }
};


}
