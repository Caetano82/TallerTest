import { useEffect, useMemo, useState } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

type TaskItem = {
	id: number;
	title: string;
	description: string;
};

const API_BASE = 'http://localhost:5000';

export default function App() {
	const [tasks, setTasks] = useState<TaskItem[]>([]);
	const [title, setTitle] = useState('');
	const [description, setDescription] = useState('');
	const [summary, setSummary] = useState('');
	const [loading, setLoading] = useState(false);

	useEffect(() => {
		(async () => {
			const res = await fetch(`${API_BASE}/api/tasks`);
			const data: TaskItem[] = await res.json();
			setTasks(data);
		})();
	}, []);

	// SignalR connection
	const connection = useMemo(() => {
		return new HubConnectionBuilder()
			.withUrl(`${API_BASE}/hubs/tasks`)
			.configureLogging(LogLevel.Information)
			.withAutomaticReconnect()
			.build();
	}, []);

	useEffect(() => {
		let isMounted = true;
		connection.on('TaskAdded', (task: TaskItem) => {
			if (!isMounted) return;
			setTasks(prev => (prev.some(t => t.id === task.id) ? prev : [...prev, task]));
		});
		connection.start().catch(err => {
			console.error('SignalR connection failed:', err);
		});
		connection.onclose(err => {
			if (err) console.error('SignalR closed with error:', err);
		});
		return () => {
			isMounted = false;
			connection.stop();
		};
	}, [connection]);

	async function addTask(e: React.FormEvent) {
		e.preventDefault();
		if (!title.trim()) return;
		setLoading(true);
		try {
			const res = await fetch(`${API_BASE}/api/tasks`, {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({ title, description })
			});
			if (!res.ok) return;
			setTitle('');
			setDescription('');
		} finally {
			setLoading(false);
		}
	}

	async function generateSummary() {
		setLoading(true);
		setSummary('');
		try {
			const res = await fetch(`${API_BASE}/api/summarize`, { method: 'POST' });
			const data = await res.json();
			setSummary(data.summary ?? '');
		} finally {
			setLoading(false);
		}
	}

	return (
		<div style={{ maxWidth: 720, margin: '2rem auto', fontFamily: 'system-ui, sans-serif' }}>
			<h2>Tasks</h2>
			<form onSubmit={addTask} style={{ display: 'grid', gap: '0.5rem', marginBottom: '1rem' }}>
				<input
					required
					placeholder="Title"
					value={title}
					onChange={e => setTitle(e.target.value)}
				/>
				<textarea
					placeholder="Description"
					value={description}
					onChange={e => setDescription(e.target.value)}
					rows={3}
				/>
				<button type="submit" disabled={loading}>Add Task</button>
			</form>

			<ul style={{ padding: 0, listStyle: 'none', display: 'grid', gap: '0.5rem' }}>
				{tasks.map(t => (
					<li key={t.id} style={{ border: '1px solid #ddd', borderRadius: 8, padding: '0.75rem' }}>
						<div style={{ fontWeight: 600 }}>{t.title}</div>
						{t.description && <div style={{ color: '#444' }}>{t.description}</div>}
					</li>
				))}
			</ul>

			<div style={{ marginTop: '1.25rem' }}>
				<button onClick={generateSummary} disabled={loading || tasks.length === 0}>Generate Summary</button>
			</div>
			{summary && (
				<div style={{ marginTop: '0.75rem', background: '#f9fafb', border: '1px solid #eee', borderRadius: 8, padding: '0.75rem' }}>
					<strong>Summary:</strong> {summary}
				</div>
			)}
		</div>
	);
}


