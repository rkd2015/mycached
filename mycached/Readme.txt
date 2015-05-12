Design
------
Server
	Puts all things together. This will start the TCP listener and listen for new
    connections. This holds the transport and Storage and dispatches request to the
	cache.

	Classes:
		MyCached
			This is the main controller class.

Transport
	This implementes the TCP connections and the is responsible for
		- Hold queued response.
		- Packet framing

	Classes:
		TcpConnection
		Packetizer		

Protocol
	This is responsible for serialization and deserialization of protocol frames.

	Classes:
		ProtocolHeader
		CommandExtras
		GetRequest
		GetResponse
		SetRequest
		SetResponse

Storage
	This implements the cache. This class handles synchronization and data storage.

	Classes:
		CacheRecord
		MemCache

Performance
-----------
Key to high perf is to enable the requests to get in and get out as fast as possible. Having granular 
locking reduces context swiches and reduces latency. A simple approach is to have a table of hash tables
and lock the inner hash table.

Keeping the number of threads in our process close to the number of cores will reduce context switching 
overhead.

The other aspect that effects performance will be the socket layer. My implementation here is naive.
Leaving this dispatch to .Net. We will end up accessing our data from any thread pool thread. 
Other options are - using epoll(linux)/IOCP(windows) which give us better control on this. 

Next aspect is memory allocation. We can end up with memory fragmentation and excessive garbage collection
overhead (in C#/Java). Here implementing a slab allocator and using thread local pools would help us.

When implementing things like expiry, we should be careful as we will sweep the table. Doing this without
effecting regular load is an interesting problem. Running long running light weight reaper in one strategy.

Limitations of my implementation
--------------------------------

1) My first level hash table does not grow dynamically. This might put some pressure as we grow the cache.

2) Socket/threading model dispatches on default thread pool.

3) Default memory allocation - would cause fragmentation.

4) Does not limit the size of a value.


Improvements/Things I would do differently
------------------------------------------

1) I would probably use C/C++ for a produciton implementation as it will give more control.

2) Implement a lock free hash table using CAS techniques. In a simple implementation, we have a table of
	linked lists. The linked lists us compare and swap loop to updating points. This would also reduce
	memory overhead of dictionaries.

3) Implement a slab allocator. To reduce fragmentation and avoid malloc locks for a C/C++ implementation.

4) Use epoll/IOCP for sockets. epoll optimizes for large number of sockets. Giving us control of multiplexing
	the traffic onto a fixed number of threads thus allowing us to optimize context switches. This is key to
	high throughput.

5) Keep track of memory usage and implement some kind of connection level throttling.

6) Better logging and live stats

7) Implement some kind of queuing to make sure we are resilient to spikes. Here the queue will also be lock free.
	Using a queue would enable us to not block socket threads avoiding back pressure on teh socket. Have a small
	number of worker threads (close to one per core), a queue per thread and assging a queue to each connection
	would optimize context swiches. 
 
8) In the socket layer, I copy to a new buffer after each socket read. I would preallocate based on the size 
	in the packet header.