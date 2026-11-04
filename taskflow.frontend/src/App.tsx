import { useEffect, useMemo, useState } from 'react'
import './App.css'

type TaskStatus = 'Pending' | 'InProgress' | 'Completed'

type Project = {
  id: string
  name: string
  description: string
  status: string
  ownerName: string
  totalTasks: number
  pendingTasks: number
  inProgressTasks: number
  completedTasks: number
}

type TaskItem = {
  id: string
  projectName: string
  title: string
  description: string
  status: TaskStatus
  priority: 'Low' | 'Medium' | 'High'
  assigneeName?: string
  updatedAt: string
}

type DashboardData = {
  totalProjects: number
  totalTasks: number
  pendingTasks: number
  inProgressTasks: number
  completedTasks: number
  completionRate: number
  projects: Project[]
  recentTasks: TaskItem[]
}

const sample: DashboardData = {
  totalProjects: 4,
  totalTasks: 18,
  pendingTasks: 6,
  inProgressTasks: 7,
  completedTasks: 5,
  completionRate: 28,
  projects: [
    {
      id: '1',
      name: 'Relatório Web',
      description: 'Entrega do sistema de gestão de tarefas e equipes.',
      status: 'Active',
      ownerName: 'Gislane Sousa',
      totalTasks: 7,
      pendingTasks: 2,
      inProgressTasks: 3,
      completedTasks: 2,
    },
    {
      id: '2',
      name: 'Onboarding',
      description: 'Processo de integração e documentação da equipe.',
      status: 'Active',
      ownerName: 'Renata Alves',
      totalTasks: 5,
      pendingTasks: 2,
      inProgressTasks: 1,
      completedTasks: 2,
    },
    {
      id: '3',
      name: 'Marketing Digital',
      description: 'Campanhas, calendário e produção de conteúdo.',
      status: 'Planning',
      ownerName: 'Helena Rocha',
      totalTasks: 3,
      pendingTasks: 1,
      inProgressTasks: 1,
      completedTasks: 1,
    },
  ],
  recentTasks: [
    {
      id: 't1',
      projectName: 'Relatório Web',
      title: 'Definir arquitetura da API',
      description: 'Mapear endpoints, fluxos e autenticação JWT para a aplicação.',
      status: 'InProgress',
      priority: 'High',
      assigneeName: 'Gislane Sousa',
      updatedAt: '2026-11-04T16:00:00Z',
    },
    {
      id: 't2',
      projectName: 'Onboarding',
      title: 'Criar dashboard inicial',
      description: 'Montar indicadores e filtros para o andamento do projeto.',
      status: 'Pending',
      priority: 'Medium',
      assigneeName: 'Renata Alves',
      updatedAt: '2026-11-03T14:30:00Z',
    },
    {
      id: 't3',
      projectName: 'Marketing Digital',
      title: 'Revisar calendário editorial',
      description: 'Validar campanhas e prazos das publicações.',
      status: 'Completed',
      priority: 'Low',
      assigneeName: 'Helena Rocha',
      updatedAt: '2026-11-02T09:15:00Z',
    },
  ],
}

function App() {
  const [dashboard, setDashboard] = useState<DashboardData>(sample)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const loadDashboard = async () => {
      try {
        const response = await fetch('/api/dashboard', { headers: { Accept: 'application/json' } })
        if (!response.ok) {
          throw new Error('API not available')
        }

        const data = (await response.json()) as DashboardData
        setDashboard(data)
      } catch {
        setDashboard(sample)
      } finally {
        setLoading(false)
      }
    }

    void loadDashboard()
  }, [])

  const metrics = useMemo(
    () => [
      { label: 'Projetos', value: dashboard.totalProjects, tone: 'blue' },
      { label: 'Tarefas', value: dashboard.totalTasks, tone: 'purple' },
      { label: 'Em andamento', value: dashboard.inProgressTasks, tone: 'amber' },
      { label: 'Concluídas', value: dashboard.completedTasks, tone: 'green' },
    ],
    [dashboard],
  )

  return (
    <div className="app-shell">
      <header className="topbar">
        <div>
          <p className="eyebrow">TaskFlow</p>
          <h1>Dashboard de projetos</h1>
        </div>
        <button type="button" className="primary-button">
          + Nova tarefa
        </button>
      </header>

      <main className="dashboard">
        <section className="metrics-grid">
          {metrics.map((metric) => (
            <article key={metric.label} className={`metric-card ${metric.tone}`}>
              <span>{metric.label}</span>
              <strong>{metric.value}</strong>
            </article>
          ))}
        </section>

        <section className="summary-row">
          <div className="summary-panel wide">
            <div className="panel-header">
              <h2>Resumo do andamento</h2>
              <span>{dashboard.completionRate}% concluído</span>
            </div>

            <div className="progress-track">
              <div className="progress-bar" style={{ width: `${dashboard.completionRate}%` }} />
            </div>

            <div className="status-grid">
              <div>
                <span className="status-dot pending" />
                <label>Pendente</label>
                <strong>{dashboard.pendingTasks}</strong>
              </div>
              <div>
                <span className="status-dot running" />
                <label>Em andamento</label>
                <strong>{dashboard.inProgressTasks}</strong>
              </div>
              <div>
                <span className="status-dot done" />
                <label>Concluída</label>
                <strong>{dashboard.completedTasks}</strong>
              </div>
            </div>
          </div>

          <div className="summary-panel">
            <div className="panel-header">
              <h2>Filtros</h2>
            </div>
            <ul className="filter-list">
              <li>Todos os projetos</li>
              <li>Equipe de design</li>
              <li>Últimos 7 dias</li>
              <li>Prioridade alta</li>
            </ul>
          </div>
        </section>

        <section className="content-grid">
          <div className="panel">
            <div className="panel-header">
              <h2>Projetos ativos</h2>
              <button type="button" className="ghost-button">
                Ver todos
              </button>
            </div>

            <div className="project-list">
              {dashboard.projects.map((project) => (
                <article key={project.id} className="project-card">
                  <div className="project-topline">
                    <h3>{project.name}</h3>
                    <span className="pill">{project.status}</span>
                  </div>
                  <p>{project.description}</p>
                  <div className="project-meta">
                    <span>Responsável: {project.ownerName}</span>
                    <span>{project.totalTasks} tarefas</span>
                  </div>
                  <div className="mini-stats">
                    <span>{project.pendingTasks} pendentes</span>
                    <span>{project.inProgressTasks} em curso</span>
                    <span>{project.completedTasks} concluídas</span>
                  </div>
                </article>
              ))}
            </div>
          </div>

          <div className="panel">
            <div className="panel-header">
              <h2>Quadro de tarefas</h2>
              <span className="muted">{loading ? 'Carregando...' : 'Atualizado agora'}</span>
            </div>

            <div className="task-board">
              {(['Pending', 'InProgress', 'Completed'] as TaskStatus[]).map((status) => (
                <div key={status} className="task-column">
                  <h3>{status === 'Pending' ? 'Pendente' : status === 'InProgress' ? 'Em andamento' : 'Concluída'}</h3>
                  {dashboard.recentTasks
                    .filter((task) => task.status === status)
                    .map((task) => (
                      <article key={task.id} className="task-item">
                        <div className="task-item-header">
                          <strong>{task.title}</strong>
                          <span className={`priority ${task.priority.toLowerCase()}`}>{task.priority}</span>
                        </div>
                        <p>{task.description}</p>
                        <div className="task-meta">
                          <span>{task.projectName}</span>
                          <span>{task.assigneeName}</span>
                        </div>
                      </article>
                    ))}
                </div>
              ))}
            </div>
          </div>
        </section>
      </main>
    </div>
  )
}

export default App
