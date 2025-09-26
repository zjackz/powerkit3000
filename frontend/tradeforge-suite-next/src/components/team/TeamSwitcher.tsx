'use client';

import { Card, Segmented, Space, Typography } from 'antd';
import { useTeamContext } from '@/contexts/TeamContext';

export const TeamSwitcher = () => {
  const { teams, team, setTeamKey } = useTeamContext();

  return (
    <Card bordered={false} style={{ background: 'rgba(15,23,42,0.75)' }}>
      <Space direction="vertical" size={12} style={{ width: '100%' }}>
        <Typography.Text type="secondary">运营团队视角</Typography.Text>
        <Segmented
          block
          value={team.key}
          onChange={(value) => setTeamKey(value as string)}
          options={teams.map((profile) => ({
            label: profile.name,
            value: profile.key,
          }))}
        />
        <Typography.Paragraph style={{ marginBottom: 0, color: '#cbd5f5' }}>
          {team.description}
        </Typography.Paragraph>
      </Space>
    </Card>
  );
};
