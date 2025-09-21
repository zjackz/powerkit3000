import { useEffect } from 'react';
import { Button, Col, DatePicker, Form, Input, InputNumber, Row, Select, Space } from 'antd';
import dayjs from 'dayjs';
import { PROJECT_CATEGORIES, PROJECT_COUNTRIES, PROJECT_STATES } from '@/constants/projectOptions';
import { ProjectFilters as ProjectFiltersModel, ProjectQueryParams } from '@/types/project';

const { RangePicker } = DatePicker;

export interface ProjectFiltersProps {
  value: ProjectQueryParams;
  onChange: (filters: ProjectQueryParams) => void;
  isLoading?: boolean;
  options?: ProjectFiltersModel;
}

const sanitizeFilters = (filters: ProjectQueryParams): ProjectQueryParams => {
  const sanitized: ProjectQueryParams = {};

  Object.entries(filters).forEach(([key, val]) => {
    if (val === undefined || val === null) {
      return;
    }

    if (Array.isArray(val) && val.length === 0) {
      return;
    }

    if (typeof val === 'string' && val.trim() === '') {
      return;
    }

    (sanitized as Record<string, unknown>)[key] = val;
  });

  return sanitized;
};

const ITEM_STYLE = { marginBottom: 12 };

export const ProjectFilters = ({ value, onChange, isLoading, options }: ProjectFiltersProps) => {
  const [form] = Form.useForm();

  const stateOptions =
    options?.states?.map((option) => ({
      label: `${option.label} (${option.count.toLocaleString()})`,
      value: option.value,
    })) ?? PROJECT_STATES;

  const countryOptions =
    options?.countries?.map((option) => ({
      label: `${option.label} (${option.count.toLocaleString()})`,
      value: option.value,
    })) ?? PROJECT_COUNTRIES;

  const categoryOptions =
    options?.categories?.map((option) => ({
      label: `${option.label} (${option.count.toLocaleString()})`,
      value: option.value,
    })) ?? PROJECT_CATEGORIES;

  useEffect(() => {
    const { launchedAfter, launchedBefore, ...rest } = value;
    form.setFieldsValue({
      ...rest,
      launchedAt:
        launchedAfter && launchedBefore
          ? [dayjs(launchedAfter), dayjs(launchedBefore)]
          : undefined,
    });
  }, [value, form]);

  const handleValuesChange = () => {
    const allValues = form.getFieldsValue();
    const { launchedAt, ...rest } = allValues;

    const normalized: ProjectQueryParams = {
      ...rest,
      launchedAfter: launchedAt?.[0]?.startOf('day').toISOString(),
      launchedBefore: launchedAt?.[1]?.endOf('day').toISOString(),
    };

    onChange(sanitizeFilters(normalized));
  };

  const handleReset = () => {
    form.resetFields();
    onChange({});
  };

  return (
    <Form
      form={form}
      layout="vertical"
      size="small"
      onValuesChange={handleValuesChange}
      initialValues={{
        page: 1,
      }}
    >
      <Row gutter={[12, 12]}>
        <Col xs={24} md={12} lg={8}>
          <Form.Item name="search" label="关键词" style={ITEM_STYLE}>
            <Input placeholder="项目名称 / 创作者 / 描述" allowClear />
          </Form.Item>
        </Col>
        <Col xs={24} md={12} lg={8}>
          <Form.Item name="states" label="项目状态" style={ITEM_STYLE}>
            <Select
              mode="multiple"
              placeholder="选择状态"
              allowClear
              options={stateOptions}
            />
          </Form.Item>
        </Col>
        <Col xs={24} md={12} lg={8}>
          <Form.Item name="countries" label="国家/地区" style={ITEM_STYLE}>
            <Select
              mode="multiple"
              placeholder="选择国家"
              allowClear
              options={countryOptions}
            />
          </Form.Item>
        </Col>
        <Col xs={24} md={12} lg={8}>
          <Form.Item name="categories" label="品类" style={ITEM_STYLE}>
            <Select
              mode="multiple"
              placeholder="选择品类"
              allowClear
              options={categoryOptions}
            />
          </Form.Item>
        </Col>
        <Col xs={24} md={12} lg={8}>
          <Form.Item name="minPercentFunded" label="最低达成率 (%)" style={ITEM_STYLE}>
            <InputNumber min={0} max={1000} style={{ width: '100%' }} placeholder="例如 120" />
          </Form.Item>
        </Col>
        <Col xs={24} md={12} lg={8}>
          <Form.Item name="launchedAt" label="上线时间" style={ITEM_STYLE}>
            <RangePicker style={{ width: '100%' }} format="YYYY-MM-DD" />
          </Form.Item>
        </Col>
      </Row>
      <Space size="small" wrap>
        <Button size="small" onClick={handleReset}>
          重置
        </Button>
        <Button type="primary" size="small" loading={isLoading} onClick={handleValuesChange}>
          应用筛选
        </Button>
      </Space>
    </Form>
  );
};
