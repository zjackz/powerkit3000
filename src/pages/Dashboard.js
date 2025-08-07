import React, { useState, useEffect } from 'react';
import { Layout, Menu, Select, Card, Col, Row, Statistic, Table, Tag, Input, Spin } from 'antd';
import { ArrowUpOutlined, ArrowDownOutlined } from '@ant-design/icons';
import ProductDetailModal from '../components/ProductDetailModal';

const { Header, Content, Footer } = Layout;
const { Option } = Select;
const { Search } = Input;

// --- Mock Data ---
const mockDataSource = [
  {
    key: '1',
    asin: 'B08KH1C13B',
    title: 'Instant Pot Duo Crisp 11-in-1 Electric Pressure Cooker with Air Fryer',
    bsr: 3,
    price: 149.99,
    reviews: 45102,
    rating: 4.7,
    listedDate: '2020-10-01',
    rankChange: 5,
  },
  {
    key: '2',
    asin: 'B07W55DDFB',
    title: 'COSORI Air Fryer Oven Combo 5.8QT Max Xl',
    bsr: 1,
    price: 119.99,
    reviews: 98341,
    rating: 4.8,
    listedDate: '2019-08-15',
    rankChange: 0,
  },
  {
    key: '3',
    asin: 'B073JYC4XM',
    title: 'Lodge 6 Quart Enameled Cast Iron Dutch Oven',
    bsr: 5,
    price: 69.90,
    reviews: 53211,
    rating: 4.8,
    listedDate: '2017-06-28',
    rankChange: -2,
  },
    {
    key: '4',
    asin: 'B00006JSUB',
    title: 'Hamilton Beach Breakfast Sandwich Maker',
    bsr: 2,
    price: 29.99,
    reviews: 67890,
    rating: 4.6,
    listedDate: '2003-05-20',
    rankChange: 1,
  },
  {
    key: '5',
    asin: 'B08F3TRH43',
    title: 'Keurig K-Mini Coffee Maker, Single Serve K-Cup Pod Coffee Brewer',
    bsr: 4,
    price: 79.99,
    reviews: 78123,
    rating: 4.5,
    listedDate: '2020-08-01',
    rankChange: -1,
  },
];

// --- Table Columns ---
const columns = [
  {
    title: 'BSR',
    dataIndex: 'bsr',
    key: 'bsr',
    sorter: (a, b) => a.bsr - b.bsr,
    defaultSortOrder: 'ascend',
  },
  {
    title: 'Product Title',
    dataIndex: 'title',
    key: 'title',
  },
  {
    title: 'Price',
    dataIndex: 'price',
    key: 'price',
    sorter: (a, b) => a.price - b.price,
    render: (price) => `$${price.toFixed(2)}`,
  },
  {
    title: 'Reviews',
    dataIndex: 'reviews',
    key: 'reviews',
    sorter: (a, b) => a.reviews - b.reviews,
  },
    {
    title: 'Rating',
    dataIndex: 'rating',
    key: 'rating',
    sorter: (a, b) => a.rating - b.rating,
  },
  {
    title: 'Rank Change (24h)',
    dataIndex: 'rankChange',
    key: 'rankChange',
    sorter: (a, b) => a.rankChange - b.rankChange,
    render: (change) => {
      if (change > 0) {
        return <Tag color="green">+{change}</Tag>;
      } else if (change < 0) {
        return <Tag color="red">{change}</Tag>;
      } else {
        return <Tag color="blue">0</Tag>;
      }
    },
  },
  {
    title: 'Listed Date',
    dataIndex: 'listedDate',
    key: 'listedDate',
    sorter: (a, b) => new Date(a.listedDate) - new Date(b.listedDate),
  },
];


const Dashboard = () => {
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    // Simulate data fetching
    setTimeout(() => {
      setLoading(false);
    }, 1500);
  }, []);

  const showModal = (product) => {
    setSelectedProduct(product);
    setIsModalVisible(true);
  };

  const handleCancel = () => {
    setIsModalVisible(false);
    setSelectedProduct(null);
  };

  const handleSearch = (value) => {
      setSearchTerm(value);
  }

  const filteredData = mockDataSource.filter(item => 
    item.title.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <Layout className="layout">
      <Header style={{ display: 'flex', alignItems: 'center' }}>
        <div className="logo" style={{ color: 'white', marginRight: '24px', fontSize: '1.2em' }}>🚀 Amazon Trends</div>
        <Menu theme="dark" mode="horizontal" defaultSelectedKeys={['1']} style={{ flex: 1 }}>
          <Menu.Item key="1">Dashboard</Menu.Item>
        </Menu>
      </Header>
      <Content style={{ padding: '0 50px', marginTop: '24px' }}>
        <div style={{ background: '#fff', padding: 24, minHeight: 280 }}>
            <Row gutter={[16, 16]} align="middle" style={{ marginBottom: '24px' }}>
                <Col>
                    <strong>Select Category:</strong>
                </Col>
                <Col>
                    <Select defaultValue="home-kitchen" style={{ width: 240 }}>
                        <Option value="home-kitchen">Home & Kitchen</Option>
                        <Option value="toys-games">Toys & Games</Option>
                        <Option value="health-household">Health & Household</Option>
                    </Select>
                </Col>
                 <Col style={{ marginLeft: 'auto', textAlign: 'right' }}>
                    <Statistic title="Data Updated" value="2025-08-06 08:00 UTC" valueStyle={{ fontSize: '1em' }} />
                </Col>
            </Row>

          <div className="site-card-wrapper">
            <Spin spinning={loading}>
                <Row gutter={16}>
                <Col span={8}>
                    <Card title="🚀 Today's Top Movers" bordered={false} headStyle={{backgroundColor: '#f0f2f5'}}>
                    <Statistic
                        title="Instant Pot Duo Crisp"
                        value={5}
                        valueStyle={{ color: '#3f8600' }}
                        prefix={<ArrowUpOutlined />}
                        suffix="spots"
                    />
                    </Card>
                </Col>
                <Col span={8}>
                    <Card title="✨ Potential New Releases" bordered={false} headStyle={{backgroundColor: '#f0f2f5'}}>
                        <p><strong>New Product A</strong> - Listed 2 days ago</p>
                        <p><strong>New Product B</strong> - Listed 5 days ago</p>
                    </Card>
                </Col>
                <Col span={8}>
                    <Card title="👑 Consistent Leaders" bordered={false} headStyle={{backgroundColor: '#f0f2f5'}}>
                        <p><strong>COSORI Air Fryer</strong> - 730 days in Top 100</p>
                        <p><strong>Lodge Dutch Oven</strong> - 500 days in Top 100</p>
                    </Card>
                </Col>
                </Row>
            </Spin>
          </div>

          <div style={{ marginTop: '24px' }}>
            <Search 
                placeholder="Search by product title..." 
                onSearch={handleSearch} 
                style={{ width: 300, marginBottom: 16 }} 
                enterButton
            />
            <Table
                dataSource={filteredData}
                columns={columns}
                pagination={{ pageSize: 10 }}
                loading={loading}
                onRow={(record) => {
                    return {
                        onClick: event => { showModal(record) },
                    };
                }}
                />
          </div>
        </div>
      </Content>
      <Footer style={{ textAlign: 'center' }}>Amazon Trends Dashboard ©2025 Created by Gemini</Footer>
      <ProductDetailModal
        visible={isModalVisible}
        onCancel={handleCancel}
        product={selectedProduct}
      />
    </Layout>
  );
};

export default Dashboard;