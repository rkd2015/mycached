Design
------

Performance considerations

- Sockets and threading model
	This depends on the underlying platfrm socket implementation. In C#, messages are received on thread pool threads.
	This would mean that we will end up accessing our data from any thread pool thread. Other options are - using 
	epoll/IOCP which give us better control on this. Keeping the number of threads in our process close to the number
	of cores will reduce context switching overhed.

- Synchronization
	The cached data has to be shared across threads. To make this efficient, we have to make sure that the synchronization
	overhead is minimal. There are a few strategies that we can take here.

		- Striping the table into chunks and synchronizing the chunks
		- Use CAS primitives to make the data structure lock free
		- Reader/writer locks
		
- Memory management
	- Efficiency and fragmentation
	- Garbage collection overhead


Improvements
- Instrumentation
- Better socket implementation
	- epoll/IOCP
- Memory allocation
	For the 
- 