using BenchmarkDotNet.Running;

BenchmarkRunner.Run<RingKeyBuffer.Benchmarks.BenchmarkThreadUnsafeBuffer>();
// BenchmarkRunner.Run<RingKeyBuffer.Benchmarks.BenchmarkConcurrent>();
