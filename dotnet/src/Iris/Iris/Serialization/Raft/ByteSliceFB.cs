// automatically generated by the FlatBuffers compiler, do not modify

namespace Iris.Serialization.Raft
{

using System;
using FlatBuffers;

public sealed class ByteSliceFB : Table {
  public static ByteSliceFB GetRootAsByteSliceFB(ByteBuffer _bb) { return GetRootAsByteSliceFB(_bb, new ByteSliceFB()); }
  public static ByteSliceFB GetRootAsByteSliceFB(ByteBuffer _bb, ByteSliceFB obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public ByteSliceFB __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public ulong Index { get { int o = __offset(4); return o != 0 ? bb.GetUlong(o + bb_pos) : (ulong)0; } }
  public byte GetValue(int j) { int o = __offset(6); return o != 0 ? bb.Get(__vector(o) + j * 1) : (byte)0; }
  public int ValueLength { get { int o = __offset(6); return o != 0 ? __vector_len(o) : 0; } }
  public ArraySegment<byte>? GetValueBytes() { return __vector_as_arraysegment(6); }

  public static Offset<ByteSliceFB> CreateByteSliceFB(FlatBufferBuilder builder,
      ulong Index = 0,
      VectorOffset ValueOffset = default(VectorOffset)) {
    builder.StartObject(2);
    ByteSliceFB.AddIndex(builder, Index);
    ByteSliceFB.AddValue(builder, ValueOffset);
    return ByteSliceFB.EndByteSliceFB(builder);
  }

  public static void StartByteSliceFB(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddIndex(FlatBufferBuilder builder, ulong Index) { builder.AddUlong(0, Index, 0); }
  public static void AddValue(FlatBufferBuilder builder, VectorOffset ValueOffset) { builder.AddOffset(1, ValueOffset.Value, 0); }
  public static VectorOffset CreateValueVector(FlatBufferBuilder builder, byte[] data) { builder.StartVector(1, data.Length, 1); for (int i = data.Length - 1; i >= 0; i--) builder.AddByte(data[i]); return builder.EndVector(); }
  public static void StartValueVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(1, numElems, 1); }
  public static Offset<ByteSliceFB> EndByteSliceFB(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<ByteSliceFB>(o);
  }
};


}
