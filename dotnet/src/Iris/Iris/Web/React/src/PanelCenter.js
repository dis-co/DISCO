import React from 'react';
import Tabs from 'muicss/lib/react/tabs';
import Tab from 'muicss/lib/react/tab';
import ReactGridLayout from 'react-grid-layout';
// import {Responsive, WidthProvider} from 'react-grid-layout';
// const ResponsiveReactGridLayout = WidthProvider(Responsive);
import { WIDGETS } from './Constants'
import widgetLayouts from './data/widgetLayouts';
import WidgetCluster from './widgets/Cluster';
import WidgetCue from './widgets/Cue';
import { map } from './Util';

const cols = w => ~~(w/50);
const rowHeight = 30;

export default function PanelCenter(props) { return (
  <div id="panel-center">
    <Tabs>
      <Tab label="CLUSTER VIEW" >
        <ReactGridLayout
          className="layout"
          layout={widgetLayouts}
          cols={12}
          width={props.width}
          rowHeight={rowHeight}
          verticalCompact={false}
        >
          <div key={WIDGETS.CLUSTER}>
            <WidgetCluster info={props.info} />
          </div>
          <div key={WIDGETS.CUE}>
            {map(props.info.state.Cues, (kv, i) =>
              <WidgetCue cue={kv[1]} />
            )}
          </div>
        </ReactGridLayout>
      </Tab>
      <Tab label="GRAPH VIEW" >
        <div>
          <p>Laboris cillum ut cillum dolore velit excepteur qui ea non incididunt in officia sit magna.</p>
        </div>
      </Tab>
    </Tabs>
  </div>
)}