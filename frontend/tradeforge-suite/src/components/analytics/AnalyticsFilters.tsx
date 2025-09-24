import { useEffect } from 'react';
import { Button, Col, DatePicker, Form, Row, Select, Space } from 'antd';
import dayjs from 'dayjs';
import type { AnalyticsFilterRequest, ProjectFilters } from '@/types/project';

const { RangePicker } = DatePicker;

interface AnalyticsFiltersProps {
  options?: ProjectFilters;
  value: AnalyticsFilterRequest;
  onChange: (filters: AnalyticsFilterRequest) => void;
  loading?: boolean;
}

const sanitizeFilters = (filters: AnalyticsFilterRequest): AnalyticsFilterRequest => {
  const sanitized: AnalyticsFilterRequest = {};

  Object.entries(filters).forEach(([key, value]) => {
    if (value === undefined || value === null) {
      return;
    }

    if (Array.isArray(value) && value.length === 0) {
      return;
    }

    (sanitized as Record<string, unknown>)[key] = value;
  });

  return sanitized;
};

export const AnalyticsFilters = ({ options, value, onChange, loading }: AnalyticsFiltersProps) => {
  const [form] = Form.useForm();

  useEffect(() => {
    const { launchedAfter, launchedBefore, ...rest } = value;
    form.setFieldsValue({
      ...rest,
      launchedRange:
        launchedAfter && launchedBefore
          ? [dayjs(launchedAfter), dayjs(launchedBefore)]
          : undefined,
    });
  }, [value, form]);

  const handleValuesChange = () => {
    const { launchedRange, ...rest } = form.getFieldsValue();
    onChange(sanitizeFilters({
      ...rest,
      launchedAfter: launchedRange?.[0]?.startOf('day').toISOString(),
      launchedBefore: launchedRange?.[1]?.endOf('day').toISOString(),
    }));
  };

  const handleReset = () => {
    form.resetFields();
    onChange({});
  };

  const countryOptions = options?.countries?.map((option) => ({
    label: option.label,
    value: option.value,
  }));
  const categoryOptions = options?.categories?.map((option) => ({
    label: option.label,
    value: option.value,
  }));
  const overfundOptions = [
    { label: '≥200%', value: 200 },
    { label: '≥500%', value: 500 },
    { label: '≥1000%', value: 1000 },
  ];

  return (
    <Form form={form} layout="vertical" onValuesChange={handleValuesChange}>
      <Row gutter={[16, 16]}>
        <Col xs={24} md={12} lg={8}>
          <Form.Item name="countries" label="国家/地区">
            <Select
              mode="multiple"
              placeholder="选择国家"
              allowClear
              loading={loading}
              options={countryOptions}
            />
          </Form.Item>
        </Col>
        <Col xs={24} md={12} lg={8}>
          <Form.Item name="categories" label="品类">
            <Select
              mode="multiple"
              placeholder="选择品类"
              allowClear
              loading={loading}
              options={categoryOptions}
            />
          </Form.Item>
        </Col>
        <Col xs={24} md={12} lg={8}>
          <Form.Item name="minPercentFunded" label="超额完成度">
            <Select
              placeholder="选择阈值"
              allowClear
              loading={loading}
              options={overfundOptions}
            />
          </Form.Item>
        </Col>
        <Col xs={24} md={12} lg={8}>
          <Form.Item name="launchedRange" label="上线时间">
            <RangePicker style={{ width: '100%' }} format="YYYY-MM-DD" />
          </Form.Item>
        </Col>
      </Row>
      <Space>
        <Button onClick={handleReset}>重置筛选</Button>
      </Space>
    </Form>
  );
};
